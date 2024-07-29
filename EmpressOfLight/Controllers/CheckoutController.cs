using EmpressOfLight.Data;
using EmpressOfLight.Helpers;
using EmpressOfLight.Models;
using EmpressOfLight.Models.ViewModels;
using EmpressOfLight.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmpressOfLight.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private readonly PaypalClient _paypalClient;
        private readonly HttpClient _httpClient;
        private readonly IVnPayService _vnPayService;


        public CheckoutController(IVnPayService vnPayService, HttpClient httpClient, ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager, PaypalClient paypalClient)
        {
            _vnPayService = vnPayService;
            _paypalClient = paypalClient;
            _context = context;
            this._userManager = userManager;
            _httpClient = httpClient;
        }

        public void InitData()
        {
            var UserId = _userManager.GetUserId(User);
            var OS = _context.OrderSelects.FirstOrDefault(o => o.Id == UserId);
            var SU = _context.ShippingUnits.ToList();
            var SA = _context.ShippingAddresses.Where(c => c.Id ==  UserId).ToList();
            if (OS == null)
            {
                OrderSelect orderSelect = new OrderSelect();
                orderSelect.Code = "0";
                orderSelect.Id = UserId;
                orderSelect.ShippingUnitId = SU[0].ShippingUnitId;
                if (SA.Count <= 0)
                {
                    ShippingAddress s = new ShippingAddress();
                    s.Address = "";
                    s.Id = UserId;
                    s.FirstName = "";
                    s.Phone = "";
                    s.LastName = "";
                    _context.ShippingAddresses.Add(s);
                    _context.SaveChanges();
                    orderSelect.ShippingAddressId = s.ShippingAddressId;
                }
                _context.OrderSelects.Add(orderSelect);
                _context.SaveChanges();
            }
        }
        public IActionResult Index()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            InitData();
            CheckoutDetail CD = new CheckoutDetail();
            var UserId = _userManager.GetUserId(User);
            var OS = _context.OrderSelects
                .Include(c => c.ShippingUnit)
                .Include(s => s.ShippingAddress)
                .Include(p => p.Coupon)
                .FirstOrDefault(o => o.Id == UserId);
            var Carts = _context.Carts.Include(c => c.Size).ThenInclude(m => m.Product).Where(n => n.Id == UserId).ToList();
            var ShippingUnits = _context.ShippingUnits.ToList();
            var SA = _context.ShippingAddresses.Where(c => c.Id == UserId).ToList();

            CD.carts = Carts;
            CD.shippingUnits = ShippingUnits;
            CD.orderSelects = OS;
            CD.shippingAddresses = SA;

            return View(CD);
        }

        public IActionResult Paypal()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(); 
        }

        #region Paypal payment
        [HttpPost("/Checkout/CreateOrder")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);
            var orderselect = _context.OrderSelects
                .Include(c => c.ShippingAddress)
                .Include(p => p.ShippingUnit)
                .Include(s => s.Coupon)
                .FirstOrDefault(c => c.Id == userId);
            var TempTotal = orderselect.ShippingUnit.ShippingUnitPrice;
            foreach (var c in _context.Carts.Include(c => c.Size).Where(p => p.Id == userId))
            {
                TempTotal += c.Size.Price * c.Quantity;
            }
            if(orderselect.Coupon.IsPercent)
            {
                TempTotal = TempTotal - TempTotal * orderselect.Coupon.Discount / 100;
            }    
            else
            {
                TempTotal = TempTotal - orderselect.Coupon.Discount;
            }
            var Total = (TempTotal/25000) +"" ;
            var TienTe = "USD";
            var MaDonHang = DateTime.Now.ToString();
            try
            {
                var response = await _paypalClient.CreateOrder(Total, TienTe, MaDonHang);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new {ex.GetBaseException().Message};
                return BadRequest(error);
            }

        }

        [HttpPost("/Checkout/CaptureOrder")]
        public async Task<IActionResult> CapturePaypalOrder([FromBody] BodyRequest body, CancellationToken cancellationToken)
        {
            Console.WriteLine(body.orderID + "Vai chuong");

            try
            {
                var response = await _paypalClient.CaptureOrder(body.orderID);
                Console.WriteLine(body.orderID + "Vai chuong so 2");
                //return RedirectToAction("CashOrder", "Order", new { PaymentMethod = "Paypal"});
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(body.orderID + "Co loi nay anh em" + ex.GetBaseException().Message);
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        public class BodyRequest
        {
            public string orderID { get; set; }
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            // Thay đổi URL API và tham số theo dịch vụ bạn sử dụng
            var response = await _httpClient.GetAsync($"https://api.exchangerate-api.com/v4/latest/{fromCurrency}");
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonDocument>(data);
            var rate = json.RootElement.GetProperty("rates").GetProperty(toCurrency).GetDecimal();

            return rate;
        }
        #endregion

        public IActionResult OrderDetail()
        {
            var UserId = _userManager.GetUserId(User);
            var Carts = _context.Carts.Include(c => c.Size).ThenInclude(m => m.Product).ToList();
            return View(Carts);
        }

        [HttpPost]
        public IActionResult AddNewShippingAddress(string firstName, string lastName, string phone, string address)
        {
            var userid = _userManager.GetUserId(User);
            ShippingAddress SA = new ShippingAddress();
            SA.FirstName = firstName;
            SA.Id = userid;
            SA.LastName = lastName;
            SA.Phone = phone;
            SA.Address = address;
            _context.ShippingAddresses.Add(SA);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ChooseShippingAddress(int shippingAddressId)
        {
            var UserId = _userManager.GetUserId(User);
            var OS = _context.OrderSelects.FirstOrDefault(o => o.Id == UserId);
            OS.ShippingAddressId = shippingAddressId;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteShippingAddress(int shippingAddressId)
        {
            var SL = _context.ShippingAddresses.FirstOrDefault(s => s.ShippingAddressId == shippingAddressId);
            _context.ShippingAddresses.Remove(SL);
            var UserId = _userManager.GetUserId(User);
            var OS = _context.OrderSelects.FirstOrDefault(o => o.Id == UserId);
            var SA = _context.ShippingAddresses.Where(p => p.Id == UserId).ToList();
            OS.ShippingAddressId = SA[0].ShippingAddressId;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ChooseShippingUnit(int shippingUnitId)
        {
            var UserId = _userManager.GetUserId(User);
            var OS = _context.OrderSelects.FirstOrDefault(o => o.Id == UserId);
            OS.ShippingUnitId = shippingUnitId;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string code)
        {
            var userid = _userManager.GetUserId(User);
            var os = _context.OrderSelects.FirstOrDefault(c => c.Id == userid);
            var cd = _context.Coupons.FirstOrDefault(c => c.Code == code);
            if (cd != null)
            {
                if(cd.Stock <= 0)
                {
                    os.Code = "0";
                }    
                else 
                {
                    os.Code = code;
                    cd.Stock--;
                }
            }
            else
            {
                os.Code ="0";
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult VNPayCash()
        {
            var user = _userManager.Users.FirstOrDefault(c => c.Id == _userManager.GetUserId(User));
            var fn = user.FirstName ?? "buyer";
            var ln = user.LastName ?? "buyer";
            var pn = user.PhoneNumber ?? "00000";
            var VnPayModel = new VnPayRequestModel
            {
                Amount = _context.Products.Sum(p => p.PricePreview),
                CreatedDate = DateTime.Now,
                Description = $"{fn} {ln} {pn}",
                FullName = fn + " " + ln,
                OrderId = new Random().Next(1000, 100000)
            };
            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, VnPayModel));
        }
    }

}

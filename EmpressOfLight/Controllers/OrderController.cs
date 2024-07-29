using EmpressOfLight.Data;
using EmpressOfLight.Helpers;
using EmpressOfLight.Models;
using EmpressOfLight.Models.ViewModels;
using EmpressOfLight.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmpressOfLight.Controllers
{
    [Authorize]

    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly PaypalClient _paypalClient;

        public OrderController(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager, IVnPayService vnPayService, PaypalClient paypalClient)
        {
            _vnPayService = vnPayService;
            _context = context;
            this._userManager = userManager;
            _paypalClient = paypalClient;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            List<Order> o = new List<Order>();
            o = _context.Orders.Where(c=>c.Id == userId).ToList();
            return View(o);
        }

        public IActionResult Manage()
        {
            var o = _context.Orders.ToList();
            return View(o);
        }

        public IActionResult Detail(int OrderId)
        {
            OrderDetail detail = new OrderDetail();
            detail.Order = _context.Orders.FirstOrDefault(c => c.OrderId == OrderId);
            detail.ProductOrders = _context.ProductOrders.Include(c=>c.Size).ThenInclude(p=>p.Product).Where(c=>c.OrderId == OrderId).ToList();
            return View(detail);
        }

        [HttpPost]        
        public IActionResult ChangeStatus(int OrderId, string status)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == OrderId);
                if (order == null)
                {
                    return RedirectToAction("Manage");
                }

                order.Status = status;
                _context.SaveChanges();

                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RedirectToAction("Error"); // Xử lý khi có lỗi xảy ra
            }

        }


        public IActionResult CashOrder(string PaymentMethod)
        {
            string result = "";
            var userId = _userManager.GetUserId(User);
            var usermail = _userManager.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(this.User));
            var orderselect = _context.OrderSelects
                .Include(c=>c.ShippingAddress)
                .Include(p => p.ShippingUnit)
                .Include(s => s.Coupon)
                .FirstOrDefault(c => c.Id == userId);
            Order order = new Order();
            order.Address = orderselect.ShippingAddress.Address;
            order.FirstName = orderselect.ShippingAddress.FirstName;
            order.LastName = orderselect.ShippingAddress.LastName;
            order.Phone = orderselect.ShippingAddress.Phone;
            order.Payment = PaymentMethod;
            order.DateTime = DateTime.Now;
            order.Total = orderselect.ShippingUnit.ShippingUnitPrice;
            order.ShippingFee = orderselect.ShippingUnit.ShippingUnitPrice;
            order.Id = userId;
            order.Email = usermail.Email;
            order.Status = "Checking";
            order.Note = "";
            _context.Orders.Add(order);
            _context.SaveChanges();
            foreach (var c in _context.Carts.Include(c => c.Size).Where(p => p.Id == userId))
            {
                ProductOrder productOrder = new ProductOrder();
                productOrder.ProductOrderId = order.OrderId + "p" + c.Size.ProductId + "s" + c.Size.SizeName;
                productOrder.OrderId = order.OrderId;
                productOrder.SizeId = c.Size.SizeId;
                productOrder.Quantity = c.Quantity;
                productOrder.Price = c.Size.Price;
                _context.ProductOrders.Add(productOrder);
                order.Total += productOrder.Price * productOrder.Quantity;
            }
            if (orderselect.Coupon.IsPercent)
            {
                order.Discount =  order.Total * orderselect.Coupon.Discount / 100;
                order.Total = order.Total - order.Total * orderselect.Coupon.Discount / 100;
            }
            else
            {
                order.Discount = orderselect.Coupon.Discount;
                order.Total = order.Total - orderselect.Coupon.Discount;
            }
            if (PaymentMethod != "DeliveryOnCash")
            {
                OnlineCash OC = new OnlineCash();
                OC.Price = order.Total;
                OC.PayMethod = PaymentMethod;
                OC.OrderId = order.OrderId;
                OC.DateTime = DateTime.Now;
                _context.OnlineCash.Add(OC);
            }
            _context.SaveChanges();

            return RedirectToAction("Detail", new { OrderId = order.OrderId });
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExcecute(Request.Query);
            if (response != null) Console.WriteLine("Response null" + response.VnPayResponseCode);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("CashOrder", new { PaymentMethod = "VNPay" });

            //return View();
        }
    }
}

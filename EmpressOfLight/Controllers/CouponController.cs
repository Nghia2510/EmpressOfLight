using EmpressOfLight.Data;
using EmpressOfLight.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpressOfLight.Controllers
{
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            var cp = _context.Coupons.ToList();
            return View(cp);
        }

        public IActionResult Manage()
        {
            var cp = _context.Coupons.ToList();
            return View(cp);
        }

        [HttpPost]
        public IActionResult EditCoupon(string code, 
            bool ispercent, 
            float discount, 
            string description,
            int stock)
        {
            Console.WriteLine(ispercent + "Vai");

            Coupon coupon = _context.Coupons.FirstOrDefault(c => c.Code == code);
            if (coupon != null)
            {
                coupon.IsPercent = ispercent;
                coupon.Discount = discount;
                coupon.Description = description;
                coupon.Stock = stock;
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult AddCoupon(string code,
            bool ispercent,
            float discount,
            string description,
            int stock)
        {
            Console.WriteLine(ispercent + "Vai");
            Coupon coupon = _context.Coupons.FirstOrDefault(c => c.Code == code);
            if (coupon == null)
            {
                Coupon cp = new Coupon();
                cp.Code = code;
                cp.IsPercent = ispercent;
                cp.Discount = discount;
                cp.Description = description;
                cp.Stock = stock;
                _context.Coupons.Add(cp);
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }

        public IActionResult DeleteCoupon(string code)
        {
            Coupon coupon = _context.Coupons.FirstOrDefault(c => c.Code == code);
            _context.Coupons.Remove(coupon);
            _context.SaveChanges(true);
            return RedirectToAction("Manage");
        }
    }
}

using EmpressOfLight.Data;
using EmpressOfLight.Models;
using EmpressOfLight.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EmpressOfLight.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TwilioService _twilioService;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager, RoleManager<IdentityRole> roleManager, TwilioService twilioService)
        {
            _context = context;
            this._userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _twilioService = twilioService;

        }

        public async Task<IActionResult> Index()
        {
            if (!(await _roleManager.RoleExistsAsync("Admin")))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            var usermail = _userManager.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(this.User));
            if (usermail != null)
            {
                if (usermail.Email == "admin@gmail.com")
                {
                    EmpressOfLightUser user = await _userManager.FindByEmailAsync("admin@gmail.com");
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }

            var UserId = _userManager.GetUserId(User);
            var Carts = _context.Carts.Where(c => c.Id == UserId).ToList();
            ViewBag.ShopCount = Carts.Count;
            var product = _context.Products.Take(10).ToList();
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ContactForm(string name, string phone, string message)
        {
            string phoneNumber = "+18777804236"; // S? ?i?n tho?i nh?n tin nh?n
            string smsMessage = $"Name: {name}\nPhone: {phone}\nMessage: {message}";

            _twilioService.SendSms(phoneNumber, smsMessage);

            return RedirectToAction("ContactConfirmation");
        }

        public IActionResult ContactConfirmation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

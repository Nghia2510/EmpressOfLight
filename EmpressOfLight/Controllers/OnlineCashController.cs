using EmpressOfLight.Data;
using EmpressOfLight.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EmpressOfLight.Controllers
{

    public class OnlineCashController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        public OnlineCashController(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index(string? paymethod)
        {
            var oc = _context.OnlineCash.ToList();
            if (!paymethod.IsNullOrEmpty())
            {
                if(paymethod == "Paypal")
                {
                    oc = oc.Where(c => c.PayMethod == "Paypal").ToList();
                }  
                if(paymethod == "VNPay")
                {
                    oc = oc.Where(c => c.PayMethod == "VNPay").ToList();
                }
            }
            return View(oc);
        }
    }
}

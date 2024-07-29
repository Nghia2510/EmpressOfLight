using EmpressOfLight.Data;
using EmpressOfLight.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace EmpressOfLight.Services
{
    public class OrderService : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private IHttpContextAccessor _httpContextAccessor;

        public OrderService(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        public string CashOrder(string PaymentMethod)
        {
            string result = "";
            var userSearch = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _userManager.FindByIdAsync(userSearch).Result;
            var orderselect = _context.OrderSelects.FirstOrDefault(c => c.Id == user.Id);
            Order order = new Order();
            
            return result;
        }

    }
}

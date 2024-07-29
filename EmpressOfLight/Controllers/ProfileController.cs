using EmpressOfLight.Data;
using EmpressOfLight.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Net.WebSockets;

namespace EmpressOfLight.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private IWebHostEnvironment Environment;

        public ProfileController(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager, IWebHostEnvironment _environment)
        {
            Environment = _environment;
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var user = _userManager.Users.FirstOrDefault(c => c.Id == _userManager.GetUserId(User));
            return View(user);
        }

        public async Task<IActionResult> UpdateProfile(string firstname, string lastname, string phone, IFormFile image)
        {
            var user = _userManager.Users.FirstOrDefault(c => c.Id == _userManager.GetUserId(User));
            user.FirstName = firstname ?? "null";
            user.LastName = lastname ?? "null";
            user.PhoneNumber = phone ?? "00000000";
            if (image != null)
            {
                user.Avatar = UploadImage(image);
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        public string UploadImage(IFormFile formFile)
        {
            var userid = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(c => c.Id == _userManager.GetUserId(User));
            string ImagePath = user.Avatar;
            if (formFile != null)
            {
                string path = Path.Combine(this.Environment.WebRootPath, "avatar");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string name = userid + Path.GetExtension(formFile.FileName);
                using (var stream = new FileStream(Path.Combine(path, name), FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }
                ImagePath = "/avatar/" + name;
            }
            return ImagePath;
        }
    }
}

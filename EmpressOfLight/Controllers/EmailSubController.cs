﻿using EmpressOfLight.Data;
using EmpressOfLight.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EmpressOfLight.Controllers
{
    public class EmailSubController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;

        public EmailSubController(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(string Email)
        {
            if (!string.IsNullOrEmpty(Email))
            {
                var subscription = new EmailSub { Email = Email };

                var e = _context.EmailSub.FirstOrDefault(x => x.Email == Email);
                if (e == null)
                {
                    _context.EmailSub.Add(subscription);
                    _context.SaveChanges();
                }

                var mailchimpResponse = await SubscribeToMailchimp(Email);

                if (mailchimpResponse.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Thank you for subscribing!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to subscribe to Mailchimp." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Please enter a valid email address." });
            }
        }

        private async Task<HttpResponseMessage> SubscribeToMailchimp(string email)
        {
            var apiKey = "62e786a83a68e56c64b64d45c8d431e9-us14";
            var listId = "0d6d5aa8fc"; 
            var apiUrl = $"https://us14.api.mailchimp.com/3.0/lists/{listId}/members/";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{apiKey}")));

                var memberData = new
                {
                    email_address = email,
                    status = "subscribed"
                };

                var json = JsonConvert.SerializeObject(memberData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                return response;
            }
        }

        public IActionResult ExportEmails()
        {
            var emails = _context.EmailSub.Select(e => e.Email).ToList();

            var csvContent = new StringBuilder();
            csvContent.AppendLine("Email Address ");

            foreach (var email in emails)
            {
                var normalizedEmail = NormalizeEmail(email);
                csvContent.AppendLine($"{normalizedEmail}");
            }

            byte[] fileContents = Encoding.UTF8.GetBytes(csvContent.ToString());
            return File(fileContents, "text/csv", "emails.csv");
        }

        public IActionResult ExportUserEmails()
        {
            var users = _userManager.Users.ToList();

            var csvContent = new StringBuilder();
            csvContent.AppendLine("Email Address,First Name,Last Name,Phone Number");


            foreach (var user in users)
            {
                csvContent.AppendLine($"{user.Email},{user.FirstName},{user.LastName},{user.PhoneNumber}");
            }

            byte[] fileContents = Encoding.UTF8.GetBytes(csvContent.ToString());
            return File(fileContents, "text/csv", "users.csv");
        }

        private string NormalizeEmail(string email)
        {
            return email.Trim().ToLower();
        }
    }
}

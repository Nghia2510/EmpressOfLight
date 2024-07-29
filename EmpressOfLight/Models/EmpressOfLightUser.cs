using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmpressOfLight.Models
{
    public class EmpressOfLightUser : IdentityUser
    {
        public virtual string FirstName { get; set; } = "null";
        public virtual string LastName { get; set; } = "null";
        public virtual string Avatar { get; set; } = "https://i.pinimg.com/736x/93/49/8a/93498aef2020751f1da821516920577b.jpg";
    }
}

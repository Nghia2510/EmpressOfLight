using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpressOfLight.Models
{
    public class Coupon
    {
        [Key]
        public string Code { get; set; }
        public bool IsPercent { get; set; }
        public float Discount { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
    }
}

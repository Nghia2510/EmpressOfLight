using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpressOfLight.Models
{
    public class ShippingUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShippingUnitId { get; set; }
        public string ShippingUnitName { get; set; }
        public string ShippingUnitDescription { get; set; }
        public float ShippingUnitPrice { get; set; } = 0;
    }
}

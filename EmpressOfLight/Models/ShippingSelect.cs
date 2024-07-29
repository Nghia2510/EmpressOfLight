using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EmpressOfLight.Models
{
    public class ShippingSelect
    {
        [Key]
        [ForeignKey("EmpressOfLightUser")]
        public string Id { get; set; }
        [ForeignKey("ShippingUnit")]
        public int ShippingUnitId { get; set; }
        public ShippingUnit ShippingUnit { get; set; }
    }
}

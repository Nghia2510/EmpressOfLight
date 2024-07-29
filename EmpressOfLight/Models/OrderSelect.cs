using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EmpressOfLight.Models
{
    public class OrderSelect
    {
        [Key]
        [ForeignKey("EmpressOfLightUser")]
        public string Id { get; set; }
        [AllowNull]
        [ForeignKey("ShippingAddress")]
        public int ShippingAddressId { get; set; }

        [ForeignKey("ShippingUnit")]
        public int ShippingUnitId { get; set; }
        [ForeignKey("Coupon")]
        public string Code { get; set; }

        public Coupon Coupon { get; set; }
        public ShippingUnit ShippingUnit { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public EmpressOfLightUser EmpressOfLightUser { get; set; }
    }
}

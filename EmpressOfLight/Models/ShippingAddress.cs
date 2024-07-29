using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpressOfLight.Models
{
    public class ShippingAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShippingAddressId { get; set; }
        [ForeignKey("EmpressOfLightUser")]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public EmpressOfLightUser EmpressOfLightUser { get; set; }
    }
}

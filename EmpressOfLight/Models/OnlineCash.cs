using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpressOfLight.Models
{
    public class OnlineCash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaypalCashId { get; set; }

        public string PayMethod { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public float Price { get; set; }
        public DateTime DateTime { get; set; }
        public Order Order { get; set; }
    }
}

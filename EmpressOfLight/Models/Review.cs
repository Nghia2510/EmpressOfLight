using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EmpressOfLight.Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ReviewId { get; set; }

        [ForeignKey("EmpressOfLightUser")]
        public string Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public string Content { get; set; }
        public int Star {  get; set; }

        public DateTime DateTime { get; set; }
        public EmpressOfLightUser EmpressOfLightUser { get; set; }
        public Product Product { get; set; }
    }
}

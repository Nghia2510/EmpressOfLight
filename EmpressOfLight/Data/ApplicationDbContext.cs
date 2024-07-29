using EmpressOfLight.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EmpressOfLight.Data
{
    public class ApplicationDbContext : IdentityDbContext<EmpressOfLightUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<About> Abouts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<EmailSub> EmailSub { get; set; }
        public DbSet<ShippingUnit> ShippingUnits { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }

        public DbSet<OnlineCash> OnlineCash { get; set; }
        public DbSet<OrderSelect> OrderSelects { get; set; }

        public DbSet<Coupon> Coupons { get; set; }


    }

}

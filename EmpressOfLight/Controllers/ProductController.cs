﻿using EmpressOfLight.Data;
using EmpressOfLight.Models;
using EmpressOfLight.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;


namespace EmpressOfLight.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<EmpressOfLightUser> _userManager;
        private IWebHostEnvironment Environment;

        public ProductController(ApplicationDbContext context, UserManager<EmpressOfLightUser> userManager, IWebHostEnvironment _environment)
        {

            _context = context;
            this._userManager = userManager;
            Environment = _environment;
        }

        public IActionResult Index(int productid, string? sizename, string productname)
        {
            var UserId = _userManager.GetUserId(User);
            var Carts = _context.Carts.Where(c => c.Id == UserId).ToList();
            ViewBag.ShopCount = Carts.Count;

            var p = _context.Products.FirstOrDefault(c => c.ProductId.Equals(productid));
            var l = _context.Sizes.ToList();
            var r = _context.Reviews.Include(c => c.EmpressOfLightUser).Where(r => r.ProductId == productid).ToList();
            var rp = _context.Products.Where(c => c.CategoryId == p.CategoryId).Take(10).ToList();

            ProductDetail productDetail = new ProductDetail();
            productDetail.Reviews = r;
            int star = 0;
            if(r.Count > 0)
            {
                foreach (var c in r)
                {
                    star += c.Star;
                }
                productDetail.star = star / r.Count;
            }    
            productDetail.Product = p;
            productDetail.RelatedProducts = rp;
            productDetail.Sizes = l.Where(c => c.ProductId == productid).ToList();
            if(!sizename.IsNullOrEmpty())
            {
                productDetail.SelectedSize = productDetail.Sizes.FirstOrDefault(c => c.SizeName.Equals(sizename));
                Console.WriteLine(productDetail.SelectedSize.SizeId);
                if (productDetail.SelectedSize != null)
                {
                    productDetail.AddToCart = true;
                    productDetail.SelectSize = true;
                }
                else
                {
                    productDetail.SelectSize = false;

                }
            }
            var request = HttpContext.Request;
            var absoluteUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            ViewBag.AbsoluteUrl = absoluteUrl;
                var absoluteUrl2 = $"{request.Scheme}://{request.Host}/";
            ViewBag.AbsoluteUrl2 = absoluteUrl2;
            Console.WriteLine(absoluteUrl2);


            productDetail.CategoryName = _context.Categories.FirstOrDefault(c => c.CategoryId == p.CategoryId).CategoryName;
            return View(productDetail);
        }

        public IActionResult Manage()
        {
            var products = _context.Products.ToList();
            products.Reverse();
            return View(products);
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(c => c.ProductId == id);
            var categories = _context.Categories.ToList();
            var sizes = _context.Sizes.Where(c=>c.ProductId == id).ToList();
            ProductDetail pd = new ProductDetail();
            pd.ProductImages = _context.ProductImages.Where(c => c.ProductId == id).ToList();
            pd.Product = product;
            pd.Categories = categories;
            pd.Sizes = sizes;
            return View(pd);
        }

        [HttpPost]
        public IActionResult AddNewProductImage(string imageurl, int productid)
        {
            var pm = _context.ProductImages.ToList();
            ProductImage pi = new ProductImage();
            pi.ProductId = productid;
            pi.ProductImageUrl = imageurl;
            _context.ProductImages.Add(pi);
            _context.SaveChanges();
            return RedirectToAction("Edit", new { id = productid });
        }

        public IActionResult AddNewProduct()
        {
            Product product = new Product();
            product.ProductName = "New Product";
            product.ProductImgPreview = "https://img.lazcdn.com/g/p/dbb40302e4063570b2b6b74d3897d40b.jpg_720x720q80.jpg";
            product.ShortDescription = "This is empty product";
            product.Description = "This is empty product";
            product.Quantity = 0;
            product.CategoryId = 1;
            product.PricePreview = 0;
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult EditProduct(int ProductId, 
            string ProductName, int CategoryId, 
            string Description, string ShortDescription,
            int Quantity, float PricePreview, IFormFile formFile)
        {

			var p = _context.Products.FirstOrDefault(c => c.ProductId == ProductId);

			string ImagePath = p.ProductImgPreview;
            if (formFile != null)
            {
                Console.WriteLine("Non null");
                string path = Path.Combine(this.Environment.WebRootPath, "productimgs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string name = ProductId + Path.GetExtension(formFile.FileName);
                using (var stream = new FileStream(Path.Combine(path, name), FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }
                ImagePath = "/productimgs/" + name;
            }

            p.ProductName = ProductName;
            p.CategoryId = CategoryId;
            p.Description = Description;
            p.ShortDescription = ShortDescription;
            p.Quantity = Quantity;
            p.PricePreview = PricePreview;
            p.ProductImgPreview = ImagePath;

            _context.SaveChanges();

            return RedirectToAction("Manage");
        }

        public IActionResult DeleteProduct(int id)
        {
            var p = _context.Products.FirstOrDefault(c=>c.ProductId == id);
            _context.Products.Remove(p);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult AddNewSize(int productid, string sizename, float price, int quantity)
        {
            Console.WriteLine("Func call");
            Size size = new Size();
            size.SizeId = productid + sizename;
            size.ProductId = productid;
            size.SizeName = sizename;
            size.Quantity = quantity;
            size.Price = price;
            Console.WriteLine(size.SizeId);

            _context.Sizes.Add(size);
            _context.SaveChanges();
            return RedirectToAction("Edit", new { id = productid });
        }

        public IActionResult DeleteSize(string sizeid)
        {
            Size size = _context.Sizes.FirstOrDefault(c => c.SizeId.Equals(sizeid));
            _context.Sizes.Remove(size);
            _context.SaveChanges();
            return RedirectToAction("Edit", new {id = size.ProductId });
        }

        [HttpPost]
        public IActionResult PostReview(string content, int stars, int productid)
        {
            Console.WriteLine(stars.ToString() + " " + productid.ToString());
            var userid = _userManager.GetUserId(User);
            Review review = new Review();
            review.Content = content ?? "NoComment";
            review.Star = stars;
            review.DateTime = DateTime.Now;
            review.Id = userid;
            review.ProductId = productid;
            _context.Reviews.Add(review);
            _context.SaveChanges();
            return RedirectToAction("Index", new {productid = productid});

        }
    }
}

﻿namespace EmpressOfLight.Models.ViewModels
{
    public class ProductDetail
    {
        public Product Product = new Product();
        public bool AddToCart = false;
        public List<Size> Sizes = new List<Size>();
        public Size SelectedSize = new Size();
        public bool SelectSize = false;
        public string CategoryName;
        public List<Category> Categories = new List<Category>();
        public List<Review> Reviews = new List<Review>();
        public List<ProductImage> ProductImages = new List<ProductImage>();
        public List<Product> RelatedProducts = new List<Product>();
        public int star = 5;
    }
}

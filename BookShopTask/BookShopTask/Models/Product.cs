namespace BookShopTask.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public double CostPrice { get; set; }

        public double SellPrice { get; set; }

        public double Discount { get; set; }
        public bool IsDeleted { get; set; }

        public string Description { get; set; }
        public ICollection<ProductTag>? ProductTags { get; set; }
        public ICollection<ProductCategory>? ProductCategories { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }
    }
}

namespace BookShopTask.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductId { get; set; }
        public int Order { get; set; }
        public bool? IsCover { get; set; }
        public Product? Product { get; set; }
    }
}

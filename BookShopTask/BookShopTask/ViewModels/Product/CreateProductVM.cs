namespace BookShopTask.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double CostPrice { get; set; }
        public double SellPrice { get; set; }
        public double Discount { get; set; }
        public IFormFile CoverImage { get; set; }
        public ICollection<IFormFile>? OtherImages { get; set; }
        public List<int> TagIds { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}

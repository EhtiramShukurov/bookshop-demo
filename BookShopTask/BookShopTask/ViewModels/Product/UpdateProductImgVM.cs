using BookShopTask.Models;

namespace BookShopTask.ViewModels
{
    public class UpdateProductImgVM
    {
        public IFormFile? CoverImage { get; set; }
        public ICollection<ProductImage>? ProductImages { get; set; }
        public ICollection<IFormFile>? OtherImages { get; set; }
    }
}

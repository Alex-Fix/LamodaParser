
namespace LaShoopaParser2
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public bool IsPopular { get; set; }
        public string Sizes { get; set; }
        public string Country { get; set; }
        public string Composition { get; set; }
        public string ImgUrls { get; set; }
        public string Description { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int GenderId { get; set; }
        public Gender Gender { get; set; }
    }
}

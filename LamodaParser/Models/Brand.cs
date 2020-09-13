using System.Collections.Generic;

namespace Lamoda
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public List<Product> Products { get; set; }
        public Brand()
        {
            Products = new List<Product>();
        }
    }
}

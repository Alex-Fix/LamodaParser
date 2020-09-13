using System.Collections.Generic;

namespace Lamoda
{
    public class Gender
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; }
        public Gender()
        {
            Products = new List<Product>();
        }
    }
}

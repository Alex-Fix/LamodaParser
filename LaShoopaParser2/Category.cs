using System;
using System.Collections.Generic;
using System.Text;

namespace LaShoopaParser2
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; }
        public Category()
        {
            Products = new List<Product>();
        }
    }
}

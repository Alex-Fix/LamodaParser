using System;
using System.Collections.Generic;
using System.Text;

namespace LaShoopaParser2
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

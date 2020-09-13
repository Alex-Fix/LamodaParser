using System;
using System.Collections.Generic;
using Lamoda;

namespace ParserForLamodaUa
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start: https://www.lamoda.ua/c/4152/default-men/?is_new=1");
            DateTime start = DateTime.Now;
            List<string> pageUrls = new List<string>
            {
                "https://www.lamoda.ua/c/4152/default-men/?is_new=1"
            };
            LamodaParser parser = new LamodaParser();
            parser.ParsePagesParallel(pageUrls, "../../wwwroot/img/Products/", "/img/Products/", "../../wwwroot/img/Brands/", "/img/Brands/");
            DateTime finish = DateTime.Now;
            Console.WriteLine($"Complete...\nTime: {finish-start}");
        }
    }
}

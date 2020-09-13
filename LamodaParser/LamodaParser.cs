using System.Runtime.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lamoda
{
    public class LamodaParser
    {
        Random rnd = new Random();

        public List<Product> ParsePageParallel(string pageUrl, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb)
        {
            List<Product> Products = new List<Product>();
            string MainHtml = DownloadPage(pageUrl);
            MainHtml = DelWhiteSpaces(MainHtml);

            Dictionary<string, string> ProductItems = FoundProductItems(MainHtml);

            Parallel.ForEach(ProductItems, (item) =>
            {
                Product Product = FoundProduct(item, ProductPathLocal, ProductPathDb, BrandPathLocal, BrandPathDb);
                Products.Add(Product);
            });



            return Products;
        }


        public List<Product> ParsePage(string pageUrl, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb)
        {
            List<Product> Products = new List<Product>();
            string MainHtml = DownloadPage(pageUrl);
            MainHtml = DelWhiteSpaces(MainHtml);

            Dictionary<string, string> ProductItems = FoundProductItems(MainHtml);

            foreach (KeyValuePair<string, string> item in ProductItems)
            {
                Product Product = FoundProduct(item, ProductPathLocal, ProductPathDb, BrandPathLocal, BrandPathDb);
                Products.Add(Product);
            }

            
            return Products;
        }
        public List<List<Product>> ParsePagesParallel(List<string> pageUrls, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb)
        {
            List<List<Product>> ProductsList = new List<List<Product>>();
            Parallel.ForEach(pageUrls, (page) =>
            {
                ProductsList.Add(ParsePageParallel(page, ProductPathLocal, ProductPathDb, BrandPathLocal, BrandPathDb));
            });
            return ProductsList;
        }
        public List<List<Product>> ParsePages(List<string> pageUrls, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb)
        {
            List<List<Product>> ProductsList = new List<List<Product>>();
            foreach(var page in pageUrls)
            {
                ProductsList.Add(ParsePage(page, ProductPathLocal, ProductPathDb, BrandPathLocal, BrandPathDb));
            }
            return ProductsList;
        }

        public Product FoundProduct(KeyValuePair<string, string> item, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb)
        {
            string ProductHtmlPath = item.Key;
            string ProductHtml = DownloadPage(ProductHtmlPath);
            ProductHtml = DelWhiteSpaces(ProductHtml);


            string Name = FoundName(ProductHtml);
            string PriceStr = FoundPrice(ProductHtml);
            int Price = Int32.Parse(PriceStr);
            bool IsPopular = rnd.Next(0, 100) % 2 == 1;


            string Size = FoundSize(item.Value);

            string Country = FoundCountry(ProductHtml);
            string Composition = FoundComposition(ProductHtml);

            string ImgUrls = FoundImgUrls(ProductHtml, ProductPathLocal, ProductPathDb);

            string Description = FoundDescription(ProductHtml);

            Brand Brand = FoundBrand(ProductHtml, BrandPathLocal, BrandPathDb);


            MatchCollection CatGenMC = Regex.Matches(ProductHtml, "<span itemprop=\"name\" class=\"js-breadcrumbs__item-text\">(.*?)</span>");
            Category Category = new Category
            {
                Name = CatGenMC[CatGenMC.Count - 1].Groups[1].Value
            };
            Gender Gender = new Gender
            {
                Name = CatGenMC[1].Groups[1].Value
            };

            Product Product = new Product
            {
                Name = Name,
                Price = Price,
                IsPopular = IsPopular,
                Sizes = Size,
                Country = Country,
                Composition = Composition,
                ImgUrls = ImgUrls,
                Description = Description,
                Brand = Brand,
                Category = Category,
                Gender = Gender
            };
            return Product;
        }
        public string DownloadPage(string url)
        {
            string pageHtml = string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                pageHtml = client.DownloadString(url);
            }
            return pageHtml;
        }

        public List<string> FoundProductUrls(string page)
        {
            List<string> ProductUrls = new List<string>();
            MatchCollection MC = Regex.Matches(page, "<div class=\"products-list-item(.*?)> <a href=\"(.*?)\" data-sku=\"(.*?)\" class=\"products-list-item__link link\" >");
            foreach (Match item in MC)
            {
                ProductUrls.Add($"https://www.lamoda.ua{item.Groups[2].Value}");
            }
            return ProductUrls;
        }

        public Dictionary<string, string> FoundProductItems(string page)
        {
            Dictionary<string, string> ProductItems = new Dictionary<string, string>();
            MatchCollection MC = Regex.Matches(page, "data-position=\"(.*?)\"(.*?)data-quick-type");
            foreach (Match item in MC)
            {
                string productUrl = "https://www.lamoda.ua" + Regex.Match(item.Groups[2].Value, "<a href=\"(.*?)\"").Groups[1].Value;
                ProductItems.Add(productUrl, item.Groups[2].Value);
            }
            return ProductItems;
        }
        public string FoundName(string page)
        {
            string Name = Regex.Match(page, "<div class=\"product-title__model-name\">(.*?)</div>").Groups[1].Value;
            return Name;
        }
        public string FoundPrice(string page)
        {
            string Price = Regex.Match(page, "<span class=\"product-prices__price(.*?)\"(.*?)>(.*?)</span>").Groups[3].Value.Replace(" ", "").Replace("₴", "");
            return Price;
        }
        public string FoundSize(string partOfPage)
        {
            MatchCollection MC = Regex.Matches(partOfPage, "<a data-link=\"(.*?)\" class=\"products-list-item__size-item link\">(.*?)</a>");
            string[] SizesArr = new string[MC.Count];
            for (int i = 0; i < MC.Count; i++)
            {
                SizesArr[i] = MC[i].Groups[2].Value;
            }
            string Size = SerializeJson(SizesArr);
            return Size;
        }
        public string SerializeJson(string[] arr)
        {
            string SerializeStr = string.Empty;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, arr);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    SerializeStr = sr.ReadToEnd();
                }
            }
            return SerializeStr;
        }
        public string FoundCountry(string page)
        {
            string Country = Regex.Match(page, "<span class=\"ii-product__attribute-label\"> Страна производства: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;
            return Country;
        }
        public string FoundComposition(string page)
        {
            string Composition = Regex.Match(page, "<span class=\"ii-product__attribute-label\"> Состав: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;
            return Composition;
        }
        public string FoundImgUrls(string page, string localPath, string pathDb)
        {
            MatchCollection MC = Regex.Matches(page, "src&quot;:&quot;(.*?)&quot;");
            string[] ImgUrlsArr = new string[MC.Count];
            for (int i = 0; i < MC.Count; i++)
            {
                string imgName = MC[i].Groups[1].Value.Replace("/", "_");
                ImgUrlsArr[i] = pathDb + imgName;
                string imgUrl = "https://a.lmcdn.ru/img600x866"+MC[i].Groups[1].Value;
                DownloadImg(imgUrl, localPath + imgName);
            }
            string ImgUrls = SerializeJson(ImgUrlsArr);
            return ImgUrls;
        }
        public void DownloadImg(string url, string path)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, path);
            }
        }
        public string FoundDescription(string page)
        {
            string Descriprion = Regex.Match(page, "<pre itemprop=\"description\">(.*?)</pre>").Groups[1].Value;
            return Descriprion;
        }
        public Brand FoundBrand(string page, string localPath, string pathDb)
        {
            Brand Brand = new Brand();
            Brand.Name = Regex.Match(page, "<h1 class=\"product-title__brand-name\" title=\"(.*?)\">(.*?)</h1>").Groups[2].Value;
            string BrandUrl = "https://www.lamoda.ua" + Regex.Match(page, "product-brand-url=\"(.*?)\"").Groups[1].Value;
            string BrandHtml = DownloadPage(BrandUrl);
            BrandHtml = DelWhiteSpaces(BrandHtml);
            string BrandImgUrl = Regex.Match(BrandHtml, "<img class=\"ip-banner__img\" src=\"(.*?)\"/>").Groups[1].Value;
            Brand.ImgUrl = string.Empty;
            if (BrandImgUrl != "")
            {
                DownloadImg($"https:{BrandImgUrl}", localPath+ BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_"));
                Brand.ImgUrl = pathDb + BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_"); 
            }
            return Brand;
        }
        public string DelWhiteSpaces(string text)
        {
            text = text.Replace("\t", "").Replace("\n", "");
            bool isSpaces = text.Contains("  ");
            while (isSpaces)
            {
                text = text.Replace("  ", " ");
                isSpaces = text.Contains("  ");
            }
            return text;
        }
    }
    
}

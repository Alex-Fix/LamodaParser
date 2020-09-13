using System.Runtime.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LaShoopaParser2
{
    class Parser
    {
        Random rnd = new Random();
        public void Parse()
        {
            List<Product> Products = new List<Product>();

            string MainHtmlPath = "https://www.lamoda.ua/c/4153/default-women/?is_new=1";
            string MainHtml = string.Empty;
            using(WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                MainHtml = client.DownloadString(MainHtmlPath);
            }
            
            MainHtml = DelWhiteSpaces(MainHtml);

            int j = 0;
            MatchCollection ProductPathMC = Regex.Matches(MainHtml, "<div class=\"products-list-item(.*?)> <a href=\"(.*?)\" data-sku=\"(.*?)\" class=\"products-list-item__link link\" >");
            MatchCollection ProductItem = Regex.Matches(MainHtml, "data-position=\"(.*?)\"(.*?)data-quick-type");
            
            List<Match> ProductPathList = new List<Match>();
            foreach(Match item in ProductPathMC)
            {
                ProductPathList.Add(item);
            }
            Parallel.ForEach(ProductPathList, (item) => {
                string ProductHtmlPath = "https://www.lamoda.ua" + item.Groups[2].Value;
                string ProductHtml = string.Empty;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    ProductHtml = client.DownloadString(ProductHtmlPath);
                }
                ProductHtml = DelWhiteSpaces(ProductHtml);


                string Name = Regex.Match(ProductHtml, "<div class=\"product-title__model-name\">(.*?)</div>").Groups[1].Value;
                string PriceStr = Regex.Match(ProductHtml, "<span class=\"product-prices__price(.*?)\"(.*?)>(.*?)</span>").Groups[3].Value.Replace(" ", "").Replace("₴", "");
                int Price = Int32.Parse(PriceStr);
                bool IsPopular = rnd.Next(0, 100) % 2 == 1;



                string Size = string.Empty;
                MatchCollection SizesMC = Regex.Matches(ProductItem[j].Groups[2].Value, "<a data-link=\"(.*?)\" class=\"products-list-item__size-item link\">(.*?)</a>");
                string[] SizesArr = new string[SizesMC.Count];
                for (int i = 0; i < SizesMC.Count; i++)
                {
                    SizesArr[i] = SizesMC[i].Groups[2].Value;
                }
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, SizesArr);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        Size = sr.ReadToEnd();
                    }
                }


                string Country = Regex.Match(ProductHtml, "<span class=\"ii-product__attribute-label\"> Страна производства: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;
                string Composition = Regex.Match(ProductHtml, "<span class=\"ii-product__attribute-label\"> Состав: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;

                string ImgUrls = string.Empty;
                MatchCollection ImgUrlsMC = Regex.Matches(ProductHtml, "src&quot;:&quot;(.*?)&quot;");
                string[] ImgUrlsArr = new string[ImgUrlsMC.Count];
                for (int i = 0; i < ImgUrlsMC.Count; i++)
                {
                    ImgUrlsArr[i] = $"/img/Products/{ImgUrlsMC[i].Groups[1].Value.Replace("/", "_")}";
                }
                foreach (Match ImgItem in ImgUrlsMC)
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile($"https://a.lmcdn.ru/img600x866{ImgItem.Groups[1].Value}", $"../../wwwroot/img/Products/{ImgItem.Groups[1].Value.Replace("/", "_")}");
                    }
                }
                ser = new DataContractJsonSerializer(typeof(string[]));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, ImgUrlsArr);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        ImgUrls = sr.ReadToEnd();
                    }
                }


                string Description = Regex.Match(ProductHtml, "<pre itemprop=\"description\">(.*?)</pre>").Groups[1].Value;

                Brand Brand = new Brand();
                Brand.Name = Regex.Match(ProductHtml, "<h1 class=\"product-title__brand-name\" title=\"(.*?)\">(.*?)</h1>").Groups[2].Value;
                string BrandUrl = "https://www.lamoda.ua" + Regex.Match(ProductHtml, "product-brand-url=\"(.*?)\"").Groups[1].Value;
                string BrandHtml = string.Empty;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    BrandHtml = client.DownloadString(BrandUrl);
                }
                BrandHtml = DelWhiteSpaces(BrandHtml);
                string BrandImgUrl = Regex.Match(BrandHtml, "<img class=\"ip-banner__img\" src=\"(.*?)\"/>").Groups[1].Value;
                Brand.ImgUrl = "";
                if (BrandImgUrl != "")
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile($"https:{BrandImgUrl}", $"../../wwwroot/img/Brands/{BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_")}");
                    }
                    Brand.ImgUrl = "/img/Brands/" + BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_");
                }

                MatchCollection CatGenMC = Regex.Matches(ProductHtml, "<span itemprop=\"name\" class=\"js-breadcrumbs__item-text\">(.*?)</span>");
                Category Category = new Category
                {
                    Name = CatGenMC[CatGenMC.Count - 1].Groups[1].Value
                };
                Gender Gender = new Gender
                {
                    Name = CatGenMC[1].Groups[1].Value
                };

                Products.Add(new Product
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
                });

                j++;
            });

            string ProductsJson = string.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Product>));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, Products);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    ProductsJson = sr.ReadToEnd();
                }
            }
            using(var stream = new StreamWriter("../../Products.txt"))
            {
                stream.Write(ProductsJson);
            }
            Console.WriteLine("Complete...");
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
        public void ParseSync()
        {
            List<Product> Products = new List<Product>();

            string MainHtmlPath = "https://www.lamoda.ua/c/4153/default-women/?is_new=1";
            string MainHtml = string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                MainHtml = client.DownloadString(MainHtmlPath);
            }

            MainHtml = DelWhiteSpaces(MainHtml);

            int j = 0;
            MatchCollection ProductPathMC = Regex.Matches(MainHtml, "<div class=\"products-list-item(.*?)> <a href=\"(.*?)\" data-sku=\"(.*?)\" class=\"products-list-item__link link\" >");
            MatchCollection ProductItem = Regex.Matches(MainHtml, "data-position=\"(.*?)\"(.*?)data-quick-type");
            foreach (Match item in ProductPathMC)
            {
                string ProductHtmlPath = "https://www.lamoda.ua" + item.Groups[2].Value;
                string ProductHtml = string.Empty;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    ProductHtml = client.DownloadString(ProductHtmlPath);
                }
                ProductHtml = DelWhiteSpaces(ProductHtml);


                string Name = Regex.Match(ProductHtml, "<div class=\"product-title__model-name\">(.*?)</div>").Groups[1].Value;
                string PriceStr = Regex.Match(ProductHtml, "<span class=\"product-prices__price(.*?)\"(.*?)>(.*?)</span>").Groups[3].Value.Replace(" ", "").Replace("₴", "");
                int Price = Int32.Parse(PriceStr);
                bool IsPopular = rnd.Next(0, 100) % 2 == 1;



                string Size = string.Empty;
                MatchCollection SizesMC = Regex.Matches(ProductItem[j].Groups[2].Value, "<a data-link=\"(.*?)\" class=\"products-list-item__size-item link\">(.*?)</a>");
                string[] SizesArr = new string[SizesMC.Count];
                for (int i = 0; i < SizesMC.Count; i++)
                {
                    SizesArr[i] = SizesMC[i].Groups[2].Value;
                }
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, SizesArr);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        Size = sr.ReadToEnd();
                    }
                }


                string Country = Regex.Match(ProductHtml, "<span class=\"ii-product__attribute-label\"> Страна производства: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;
                string Composition = Regex.Match(ProductHtml, "<span class=\"ii-product__attribute-label\"> Состав: </span><span class=\"ii-product__attribute-value\">(.*?)</span>").Groups[1].Value;

                string ImgUrls = string.Empty;
                MatchCollection ImgUrlsMC = Regex.Matches(ProductHtml, "src&quot;:&quot;(.*?)&quot;");
                string[] ImgUrlsArr = new string[ImgUrlsMC.Count];
                for (int i = 0; i < ImgUrlsMC.Count; i++)
                {
                    ImgUrlsArr[i] = $"/img/Products/{ImgUrlsMC[i].Groups[1].Value.Replace("/", "_")}";
                }
                foreach (Match ImgItem in ImgUrlsMC)
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile($"https://a.lmcdn.ru/img600x866{ImgItem.Groups[1].Value}", $"../../wwwroot/img/Products/{ImgItem.Groups[1].Value.Replace("/", "_")}");
                    }
                }
                ser = new DataContractJsonSerializer(typeof(string[]));
                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, ImgUrlsArr);
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        ImgUrls = sr.ReadToEnd();
                    }
                }


                string Description = Regex.Match(ProductHtml, "<pre itemprop=\"description\">(.*?)</pre>").Groups[1].Value;

                Brand Brand = new Brand();
                Brand.Name = Regex.Match(ProductHtml, "<h1 class=\"product-title__brand-name\" title=\"(.*?)\">(.*?)</h1>").Groups[2].Value;
                string BrandUrl = "https://www.lamoda.ua" + Regex.Match(ProductHtml, "product-brand-url=\"(.*?)\"").Groups[1].Value;
                string BrandHtml = string.Empty;
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    BrandHtml = client.DownloadString(BrandUrl);
                }
                BrandHtml = DelWhiteSpaces(BrandHtml);
                string BrandImgUrl = Regex.Match(BrandHtml, "<img class=\"ip-banner__img\" src=\"(.*?)\"/>").Groups[1].Value;
                Brand.ImgUrl = "";
                if (BrandImgUrl != "")
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile($"https:{BrandImgUrl}", $"../../wwwroot/img/Brands/{BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_")}");
                    }
                    Brand.ImgUrl = "/img/Brands/" + BrandImgUrl.Replace("//a.lmcdn.ru/", "").Replace("/", "_");
                }

                MatchCollection CatGenMC = Regex.Matches(ProductHtml, "<span itemprop=\"name\" class=\"js-breadcrumbs__item-text\">(.*?)</span>");
                Category Category = new Category
                {
                    Name = CatGenMC[CatGenMC.Count - 1].Groups[1].Value
                };
                Gender Gender = new Gender
                {
                    Name = CatGenMC[1].Groups[1].Value
                };

                Products.Add(new Product
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
                });

                j++;
            }

            string ProductsJson = string.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Product>));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, Products);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    ProductsJson = sr.ReadToEnd();
                }
            }
            using (var stream = new StreamWriter("../../Products.txt"))
            {
                stream.Write(ProductsJson);
            }
            Console.WriteLine("Complete...");
        }
    }
    
    
}

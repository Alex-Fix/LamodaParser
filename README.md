# LamodaParser
It's a parser for the famous Ukrainian site "Lamoda.ua". It will be used for my test project.


I created a class library for the .Net Framework for parsing products from the site "Lamoda.ua".
It will be used for my future online store test project to populate the database with products.

 I have provided 4 functions to use this library. 
************************************************************
1) For parsing the product page:
  public List<Product> ParsePage(string pageUrl, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb){***}
2) For parsing the product page using a class "Parallel":
  public List<Product> ParsePageParallel(string pageUrl, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb){***}
3) For parsing several pages with goods:
  public List<List<Product>> ParsePages(List<string> pageUrls, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb){***}
4) For parsing multiple pages with goods using a class "Parallel":
  public List<List<Product>> ParsePagesParallel(List<string> pageUrls, string ProductPathLocal, string ProductPathDb, string BrandPathLocal, string BrandPathDb){***}

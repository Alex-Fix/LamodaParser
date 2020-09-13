using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lamoda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamoda.Tests
{
    [TestClass()]
    public class LamodaParserTests
    {
       
       
        [TestMethod()]
        public void FoundNameTest()
        {
            //Arrange
            LamodaParser parser = new LamodaParser();
            string expected = "Куртка утепленная ";
            string html = "<div class=\"product-title__model-name\">Куртка утепленная </div>";
            //Act
            string result = parser.FoundName(html);
            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void FoundPriceTest()
        {
            //Arrange
            LamodaParser parser = new LamodaParser();
            string expected = "1790";
            string html = "<span class=\"product-prices__price product-prices__price_old\">1 790</span>";
            //Act
            string result = parser.FoundPrice(html);
            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void FoundCountryTest()
        {
            //Arrange
            LamodaParser parser = new LamodaParser();
            string expected = " Украина ";
            string html = " 30 дней </span></div><div class=\"ii-product__attribute hidden\"><span class=\"ii-product__attribute-label\"> Страна производства: </span><span class=\"ii-product__attribute-value\"> Украина </span></div><div class=\"ii-product__attribute hi";
            //Act
            string result = parser.FoundCountry(html);
            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void FoundCompositionTest()
        {
            //Arrange
            LamodaParser parser = new LamodaParser();
            string expected = " Материал 1: Полиэстер - 60%, Шелк - 20%, Вискоза - 20%; Материал 2: Полиамид - 96%, Эластан - 4% ";
            string html = "ext\"><div class=\"ii-product__attributes\"><div class=\"ii-product__attribute\"><span class=\"ii-product__attribute-label\"> Состав: </span><span class=\"ii-product__attribute-value\"> Материал 1: Полиэстер - 60%, Шелк - 20%, Вискоза - 20%; Материал 2: Полиамид - 96%, Эластан - 4% </span></div><div class=\"ii-product__attribute\"><span class=\"ii-product__attribute-label\"> Материал подкладки: </span";
            //Act
            string result = parser.FoundComposition(html);
            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void DelWhiteSpacesTest()
        {
            //Arrange
            LamodaParser parser = new LamodaParser();
            string expected = "Hi Alex";
            string html = "\n\nHi  \n\tAlex\n\n\n\n\n";
            //Act
            string result = parser.DelWhiteSpaces(html);
            //Assert
            Assert.AreEqual(expected, result);
        }
    }
}
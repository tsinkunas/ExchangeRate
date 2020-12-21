using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleAppExchangeRate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();

            Console.WriteLine("Įveskite norimą datą (pvz: 2014.01.01)");
            DateTime insertDate;
            DateTime.TryParse(Console.ReadLine(), out insertDate);
            DateTime max = new DateTime(2014,12,31);

            if (insertDate <= max)
            {
                string customDate = "yyyy.MM.dd";
                string selectedDay = insertDate.ToString(customDate);
                string selectedDayBefore = insertDate.AddDays(-1).ToString(customDate);
                Console.WriteLine($"Pasirinkta diena: {selectedDay}\n" +
                    $"Ankstesnė diena: {selectedDayBefore}");

                var responce = await httpClient.GetAsync("http://www.lb.lt//webservices/ExchangeRates/ExchangeRates.asmx/getExchangeRatesByDate?Date=" + selectedDay);
                var responceBefore = await httpClient.GetAsync("http://www.lb.lt//webservices/ExchangeRates/ExchangeRates.asmx/getExchangeRatesByDate?Date=" + selectedDayBefore);

                var responceBody = await responce.Content.ReadAsStringAsync();
                var responceBodyBefore = await responceBefore.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                var htmlDocBefore = new HtmlDocument();
                htmlDoc.LoadHtml(responceBody);
                htmlDocBefore.LoadHtml(responceBodyBefore);

                var currencies = htmlDoc.DocumentNode.Descendants("item");
                var currenciesBefore = htmlDocBefore.DocumentNode.Descendants("item");
                
                List<ExchangeRates> exchangeRates = new List<ExchangeRates>();
                List<ExchangeRatesBefore> exchangeRatesBefore = new List<ExchangeRatesBefore>();

                foreach (HtmlNode currency in currencies)
                {
                    var currencyName = currency.SelectSingleNode("./currency").InnerHtml;

                    var currencyRate = currency.SelectSingleNode("./rate").InnerHtml;

                    exchangeRates.Add(new ExchangeRates() {Currency = currencyName, Rate = currencyRate});
                }
                foreach (HtmlNode currencyBefore in currenciesBefore)
                {
                    var currencyName = currencyBefore.SelectSingleNode("./currency").InnerHtml;

                    var currencyRate = currencyBefore.SelectSingleNode("./rate").InnerHtml;

                    exchangeRatesBefore.Add(new ExchangeRatesBefore() { Currency = currencyName, Rate = currencyRate });
                }
                foreach (ExchangeRates exchangeRate in exchangeRates)
                {
                    foreach (ExchangeRatesBefore exchangeRateBefore in exchangeRatesBefore)
                    {
                        if (exchangeRate.Currency == exchangeRateBefore.Currency)
                        {
                            double d = Double.Parse(exchangeRate.Rate) - Double.Parse(exchangeRateBefore.Rate);
                            double diference = Math.Round(d, 2);
                            Console.WriteLine($"Valiuta: {exchangeRate.Currency}, šios dienos kursas: {exchangeRate.Rate}, pokytis nuo vakar: {diference}");
                        }
                    }
                }
                
            }
            else
            {
                Console.WriteLine("Įvedėte blogą datą");
            }            
        }
    }
}

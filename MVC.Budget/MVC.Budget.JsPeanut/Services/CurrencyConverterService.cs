﻿using MVC.Budget.JsPeanut.Models;
using RestSharp;
using System.Text.Json;

namespace MVC.Budget.JsPeanut.Services
{
    public class CurrencyConverterService
    {
        public decimal ConvertValueToCategoryCurrency(string transactionCurrencyCode, decimal transactionValue, string categoryCurrencyCode)
        {
            try
            {
				var client = new RestClient("http://api.exchangeratesapi.io/");
				var requestUrl = "v1/latest?access_key=69b84dacef3c17cfd0da06b728d32c65&symbols=";
				if (transactionCurrencyCode != null && categoryCurrencyCode != null)
				{
					requestUrl = requestUrl + $"{transactionCurrencyCode}" + $",{categoryCurrencyCode}";
				}
				var request = new RestRequest(requestUrl);
				var response = client.ExecuteGet(request);

				var rates = JsonSerializer.Deserialize<ApiCurrencyRate>(response.Content);

				var oneEuroToTransactionCurrency = rates.Rates.Where(x => x.Key == transactionCurrencyCode).First().Value;
				var oneEuroToCategoryCurrency = rates.Rates.Where(x => x.Key == categoryCurrencyCode).First().Value;

				decimal oneEuro = 1;
				decimal oneOfTransactionCurrency = 1;

				//x of Transaction Currency are y Euro
				var oneTransactionCurrencyToEuro = (oneOfTransactionCurrency * oneEuro) / oneEuroToTransactionCurrency;

				decimal oneOfCategoryCurrency = 1;

				//x of Category Currency are y Euro
				var oneCategoryCurrencyToEuro = (oneOfCategoryCurrency * oneEuro) / oneEuroToCategoryCurrency;

				//Knowing this now we can calculate the rate of transaction and category currencies.
				//If x amount of euros are equal to 1 unit category currency, y amount of euros (1 unit of transaction currency) are equal to z of category currency
				var transactionCurrency_categoryCurrency_Rate = (oneTransactionCurrencyToEuro * oneOfCategoryCurrency) / oneCategoryCurrencyToEuro;

				transactionValue = transactionValue * transactionCurrency_categoryCurrency_Rate;

				return transactionValue;
			}
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

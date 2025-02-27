using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Web_API.Models;

namespace Web_API.Services
{
    public class MakeupApiService
    {
        private const string BaseUrl = "https://makeup-api.herokuapp.com/api/v1/products";
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                string jsonResponse = await _httpClient.GetStringAsync(BaseUrl + ".json");
                return JsonConvert.DeserializeObject<List<Product>>(jsonResponse);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Product>> GetFilteredProductsAsync(string brand = null, string productType = null)
        {
            try
            {
                StringBuilder queryParams = new StringBuilder();

                if (!string.IsNullOrEmpty(brand))
                {
                    queryParams.Append($"?brand={Uri.EscapeDataString(brand)}");
                }

                if (!string.IsNullOrEmpty(productType))
                {
                    if (queryParams.Length == 0)
                        queryParams.Append($"?product_type={Uri.EscapeDataString(productType)}");
                    else
                        queryParams.Append($"&product_type={Uri.EscapeDataString(productType)}");
                }

                string url = BaseUrl + ".json" + queryParams.ToString();
                string jsonResponse = await _httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<Product>>(jsonResponse);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> GetImageBytesAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return null;

                using (HttpResponseMessage response = await _httpClient.GetAsync(imageUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
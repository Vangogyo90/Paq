using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paq.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Paq.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public static List<ColorsInDelivery> colorsInDeliveries;

        public async Task<ViewResult> Cart()
        {
            List<City> cities = await GetCities();
            ViewBag.Cities = cities;
            return View(colorsInDeliveries);
        }

        [HttpGet]
        public async Task<IActionResult> MyDeliveryUpdate(int IdDelivery, string Adress, string Salt, int CityId, int StatusOrderId, int UserId)
        {
            try
            {
                Delivery delivery = new Delivery();
                delivery.IdDelivery = IdDelivery;
                delivery.Adress = Adress;
                delivery.Salt = Salt;
                delivery.CityId = CityId;
                delivery.StatusOrderId = StatusOrderId;
                delivery.UserId = UserId;

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(delivery), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PutAsync($"http://localhost:7192/api/Deliveries/{IdDelivery}", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("MyDeliveries");
                        }
                    }
                }
                return RedirectToAction("MyDeliveries");
            }
            catch
            {
                return RedirectToAction("MyDeliveries");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCartNotAll(int quantityid, int quantityToRemove)
        {
            try
            {
                var itemToRemove = colorsInDeliveries.FirstOrDefault(item => item.Color.IdQuantityColors == quantityid);

                if (itemToRemove != null)
                {
                    if (quantityToRemove == 0)
                    {
                        colorsInDeliveries.Remove(itemToRemove);
                    }
                    else
                    {
                        if (itemToRemove.Quantity > quantityToRemove)
                        {
                            itemToRemove.Quantity -= quantityToRemove;
                            if (itemToRemove.Quantity <= 0)
                            {
                                colorsInDeliveries.Remove(itemToRemove);
                            }
                        }
                        else
                        {
                            colorsInDeliveries.Remove(itemToRemove);
                        }
                    }
                }

                return RedirectToAction("Cart");
            }
            catch
            {
                return RedirectToAction("Cart");
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddCart(int Quantity, int quantityid, int pageCount)
        {
            try
            {
                if (Quantity == 0)
                {
                    Quantity = 1;
                }

                if (colorsInDeliveries == null)
                {
                    colorsInDeliveries = new List<ColorsInDelivery>();
                }

                QuantityColor productDetails = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/GetDataByID/{quantityid}", HttpCompletionOption.ResponseHeadersRead))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        productDetails = JsonConvert.DeserializeObject<QuantityColor>(apiResponse);
                    }
                }

                Discount discount = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"http://localhost:7192/api/Discounts/GetDiscountByColor/{productDetails.ColorId}", HttpCompletionOption.ResponseHeadersRead))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        discount = JsonConvert.DeserializeObject<Discount>(apiResponse);
                    }
                }

                var existingItem = colorsInDeliveries.FirstOrDefault(item => item.Color.ColorId == productDetails.ColorId);

                if (existingItem != null)
                {
                    existingItem.Quantity += Quantity;
                }
                else
                {
                    var newItem = new ColorsInDelivery
                    {
                        Quantity = Quantity,
                        Color = productDetails,
                        Discount = discount
                    };

                    colorsInDeliveries.Add(newItem);
                }

                return RedirectToAction("Shop", new { pageCount });
            }
            catch
            {
                return RedirectToAction("Shop", new { pageCount });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStocksCart(int Quantity, int quantityid, int pageCount)
        {
            try
            {
                if (Quantity == 0)
                {
                    Quantity = 1;
                }

                if (colorsInDeliveries == null)
                {
                    colorsInDeliveries = new List<ColorsInDelivery>();
                }

                QuantityColor productDetails = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/GetDataByID/{quantityid}", HttpCompletionOption.ResponseHeadersRead))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        productDetails = JsonConvert.DeserializeObject<QuantityColor>(apiResponse);
                    }
                }

                Discount discount = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync($"http://localhost:7192/api/Discounts/GetDiscountByColor/{productDetails.ColorId}", HttpCompletionOption.ResponseHeadersRead))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        discount = JsonConvert.DeserializeObject<Discount>(apiResponse);
                    }
                }

                var existingItem = colorsInDeliveries.FirstOrDefault(item => item.Color.ColorId == productDetails.ColorId);

                if (existingItem != null)
                {
                    existingItem.Quantity += Quantity;
                }
                else
                {
                    var newItem = new ColorsInDelivery
                    {
                        Quantity = Quantity,
                        Color = productDetails,
                        Discount = discount
                    };

                    colorsInDeliveries.Add(newItem);
                }

                return RedirectToAction("Stocks", new { pageCount });
            }
            catch
            {
                return RedirectToAction("Stocks", new { pageCount });
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Paint()
        {
            return View();
        }

        public ViewResult CallBack() => View();

        [HttpPost]
        public async Task<IActionResult> CallBack(FeedBack feedback, string messageBytes)
        {
            byte[] bytes = messageBytes.Split(',').Select(byte.Parse).ToArray();
            string message = Encoding.UTF8.GetString(bytes);

            feedback.Message = Encoding.UTF8.GetBytes(message);

            FeedBack feed = new FeedBack();
            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(feedback), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync("http://localhost:7192/api/FeedBacks", content))
                {
                    string apiResonse = await response.Content.ReadAsStringAsync();
                    feed = JsonConvert.DeserializeObject<FeedBack>(apiResonse);
                }
            }
            return View(feed);
        }

        public async Task<List<City>> GetCities()
        {
            List<City> cities = new List<City>();

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("http://localhost:7192/api/Cities"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    cities = JsonConvert.DeserializeObject<List<City>>(apiResponse);
                }
            }

            return cities;
        }

        [HttpPost]
        public async Task<IActionResult> Cart(string Adress, int CityId, int StatusOrderId, int UserId)
        {
            Delivery delivery = new Delivery();
            delivery.Adress = Adress;
            delivery.CityId = CityId;
            delivery.StatusOrderId = StatusOrderId;
            delivery.UserId = UserId;

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(delivery), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync("http://localhost:7192/api/Deliveries", content))
                {
                    string apiResonse = await response.Content.ReadAsStringAsync();
                    delivery = JsonConvert.DeserializeObject<Delivery>(apiResonse);

                    ColorDelivery colorsInDelivery = new ColorDelivery();
                    colorsInDelivery.DeliveryId = delivery.IdDelivery;

                    for (int i = 0; i < colorsInDeliveries.Count(); i++)
                    {
                        colorsInDelivery.ColorId = colorsInDeliveries[i].Color.ColorId;
                        for (int q = 0; q < colorsInDeliveries[i].Quantity; q++)
                        {
                            StringContent contentColors = new StringContent(JsonConvert.SerializeObject(colorsInDelivery), Encoding.UTF8, "application/json");
                            using (var responseColor = await httpClient.PostAsync("http://localhost:7192/api/ColorDeliveries", contentColors))
                            {
                                string apiResonseColors = await responseColor.Content.ReadAsStringAsync();
                            }
                        }
                    }
                    colorsInDeliveries.Clear();
                }
            }
            return View();
        }

        public async Task<IActionResult> Shop(int pageCount)
        {
            Pagination paginations = new Pagination();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/Pagination?pageNumber={pageCount}&pageSize=27", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    paginations = JsonConvert.DeserializeObject<Pagination>(apiResponse);
                }
            }

            return View(paginations);
        }

        public async Task<IActionResult> MyDeliveries()
        {
            List<MyDelivery> myDeliveries = new List<MyDelivery>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/Deliveries/GetDataByUser?idUser={User.Identity.Name}", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var Delivery = JsonConvert.DeserializeObject<List<Delivery>>(apiResponse);

                    string apiResponseColor = "";
                    string apiResponsePrice = "";
                    for (int i = 0; i < Delivery.Count(); i++)
                    {
                        using (var responseColor = await httpClient.GetAsync($"http://localhost:7192/api/ColorDeliveries/GetData/{Delivery[i].IdDelivery}", HttpCompletionOption.ResponseHeadersRead))
                        {
                            apiResponseColor = await responseColor.Content.ReadAsStringAsync();
                            var colorDeliveryColor = JsonConvert.DeserializeObject<List<ColorDelivery>>(apiResponseColor);

                            using (var responsePrice = await httpClient.GetAsync($"http://localhost:7192/api/Price/CalculateTotalPriceForDelivery/{Delivery[i].IdDelivery}", HttpCompletionOption.ResponseHeadersRead))
                            {
                                apiResponsePrice = await responsePrice.Content.ReadAsStringAsync();
                                var colorDeliveryPrice = JsonConvert.DeserializeObject<double>(apiResponsePrice);
                                Delivery[i].PriceAll = colorDeliveryPrice;
                            }

                            myDeliveries.Add(new MyDelivery { delivery = Delivery[i], colorDeliveries = colorDeliveryColor });
                        }
                    }
                }
            }
            return View(myDeliveries);
        }

        public async Task<IActionResult> News()
        {
            List<News> news = new List<News>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/News", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    news = JsonConvert.DeserializeObject <List<News>>(apiResponse);
                }
            }

            return View(news);
        }

        public async Task<IActionResult> Stocks(int pageCount)
        {
            Pagination paginations = new Pagination();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/PaginationStocks?pageNumber={pageCount}&pageSize=27", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    paginations = JsonConvert.DeserializeObject<Pagination>(apiResponse);
                }
            }

            return View(paginations);
        }

        [HttpGet("ShopByID/{id}")]
        public async Task<IActionResult> ShopByID(int id)
        {
            QuantityColor productDetails = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/GetDataByID/{id}", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    productDetails = JsonConvert.DeserializeObject<QuantityColor>(apiResponse);
                }
            }

            if (productDetails == null)
            {
                return NotFound();
            }

            return View(productDetails);
        }

        [HttpGet("PDFShop/{id}")]
        public async Task<IActionResult> PDFShop(int id)
        {
            Color color = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/Colors/{id}", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    color = JsonConvert.DeserializeObject<Color>(apiResponse);
                }
            }

            if (color == null)
            {
                return NotFound();
            }

            return View(color);
        }

        [HttpGet("NewsByID/{id}")]
        public async Task<IActionResult> NewsByID(int id)
        {
            NewsPhoto newsPhoto = null;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/News/NewsPhoto/{id}", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    newsPhoto = JsonConvert.DeserializeObject<NewsPhoto>(apiResponse);
                }
            }

            if (newsPhoto == null)
            {
                return NotFound();
            }

            return View(newsPhoto);
        }

        public IActionResult SearchShop(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Shop");
            }

            Pagination searchResults = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/Search?searchQuery={query}?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    searchResults = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Shop", searchResults);
        }

        public IActionResult SearchStocks(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Stocks");
            }

            Pagination searchResults = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/SearchStocks?searchQuery={query}?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    searchResults = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Stocks", searchResults);
        }

        public IActionResult UpShop()
        {
            Pagination results = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/UpPrice?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    results = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Shop", results);
        }

        public IActionResult UpPriceStocks()
        {
            Pagination results = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/UpPriceStocks?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    results = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Stocks", results);
        }

        public IActionResult DownShop()
        {
            Pagination results = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/DownPrice?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    results = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Shop", results);
        }

        public IActionResult DownPriceStocks()
        {
            Pagination results = new Pagination();
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync($"http://localhost:7192/api/QuantityColors/DownPriceStocks?pageNumber=1&pageSize=10000").Result;

                if (response.IsSuccessStatusCode)
                {
                    results = JsonConvert.DeserializeObject<Pagination>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return View("Stocks", results);
        }

        public IActionResult Laborathory()
        {
            return View();
        }

        public async Task<IActionResult> Palette()
        {
            List<RalCatalog> ralCatalogs = new List<RalCatalog>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"http://localhost:7192/api/RalCatalogs", HttpCompletionOption.ResponseHeadersRead))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ralCatalogs = JsonConvert.DeserializeObject<List<RalCatalog>>(apiResponse);
                }
            }

            return View(ralCatalogs);
        }

        public IActionResult Production()
        {
            return View();
        }
    }
}
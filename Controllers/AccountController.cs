using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paq.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Paq.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("Account")]
        public IActionResult Account(string returnUrl = "/")
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpGet]
        [Route("Register")]
        [ActionName("Register")]
        public ViewResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User user)
        {
            user.RoleId = 6;
            User users = new User();
            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync("http://localhost:7192/api/Users", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResonse = await response.Content.ReadAsStringAsync();
                        users = JsonConvert.DeserializeObject<User>(apiResonse);
                    }
                    else
                    {
                        ModelState.AddModelError("Error Auth", "Ошибка при регистрации пользователя. Пожалуйста, проверьте введенные данные.");
                        return View("Register", user);
                    }
                }
            }
            return View("~/Views/Home/Index.cshtml", users);
        }

        [HttpPost]
        [Route("Account")]
        public async Task<IActionResult> Account(LoginViewModel model)
        {
            var client = _httpClientFactory.CreateClient();

            using (var response = await client.GetAsync($"http://localhost:7192/api/Users/{model.Username}/{model.Password}", HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseID = await client.GetAsync($"http://localhost:7192/api/Users/UserIDByLogin/{model.Username}", HttpCompletionOption.ResponseHeadersRead);
                    string ID = await responseID.Content.ReadAsStringAsync();
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var claims = new[]
                    {
                        new Claim(
                            ClaimTypes.Name, ID,
                            ClaimTypes.NameIdentifier, apiResponse
                        )
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return LocalRedirect(model.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }
            }
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Account");
        }
    }

}

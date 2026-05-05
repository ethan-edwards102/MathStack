using System.Text;
using MathApiClient.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MathApiClient.Controllers;

public class AuthController : Controller
{
    private static HttpClient _http;

    public AuthController(IConfiguration config)
    {
        if (_http == null)
        {
            var baseUrl = config["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("ApiSettings:BaseUrl is missing");
            }
            
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(LoginModel model)
    {
        var content = new StringContent
        (
            JsonConvert.SerializeObject(model),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _http.PostAsync("api/Auth/Register", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<AuthResponse>(json);
            
            if (body == null)
            {
                ViewBag.Result = "No auth response from server";
                return View();
            }
            
            HttpContext.Session.SetString("currentUser", body.UserId);
            HttpContext.Session.SetString("MathJWT", body.Token);
            
            return RedirectToAction("Calculate", "Math");
        }
        
        ViewBag.Result = response.Content.ReadAsStringAsync().Result;
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var content = new StringContent
        (
            JsonConvert.SerializeObject(model),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _http.PostAsync("api/Auth/Login", content);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<AuthResponse>(json);
            
            if (body == null)
            {
                ViewBag.Result = "No auth response from server";
                return View();
            }

            HttpContext.Session.SetString("currentUser", body.UserId);
            HttpContext.Session.SetString("MathJWT", body.Token);
            return RedirectToAction("Calculate", "Math");
        }
        
        ViewBag.Result = response.Content.ReadAsStringAsync().Result;
        return View();
    }
    
    [HttpGet]
    public IActionResult LogOut()
    {
        HttpContext.Session.Remove("currentUser");
        HttpContext.Session.Remove("MathJWT");
        
        _http.DefaultRequestHeaders.Authorization = null;

        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return RedirectToAction("Login");
    }
}
using System.Text;
using MathApiClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace MathApiClient.Controllers;

public class MathController : Controller
{
    private static HttpClient _http;

    public MathController(IConfiguration config)
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
    
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Calculate()
    {
        if (!ValidateLogin())
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.Operations = GetOperations();

        return View();
    }
    
    [
        HttpPost,
        ValidateAntiForgeryToken,
        ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)
    ]
    public async Task<IActionResult> Calculate
    (
        decimal? FirstNumber,
        decimal? SecondNumber,
        int Operation
    )
    {
        var token = HttpContext.Session.GetString("MathJWT");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var user = HttpContext.Session.GetString("currentUser");

        decimal? result = 0;
        MathCalculation mathCalculation;

        try
        {
            mathCalculation = MathCalculation.Create(FirstNumber, SecondNumber, Operation, result, user);
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;
            ViewBag.Operation = GetOperations();
            return View();
        }

        var content = new StringContent
        (
            JsonConvert.SerializeObject(mathCalculation),
            Encoding.UTF8,
            "application/json"
        );
        
        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        
        var response = await _http.PostAsync("api/Math/PostCalculate", content);
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<MathCalculation>(json);

            ViewBag.Operations = GetOperations();
            ViewBag.Result = body.Result;
            
            return View();
        }

        ViewBag.Result = "An error has occured";
        return View();
    }
    
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> History()
    {
        var token = HttpContext.Session.GetString("MathJWT");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        // Preserve message after history is cleared
        if (TempData["ClearMessage"] is string message)
        {
            ViewBag.ClearMessage = message;
        }
        
        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        
        var response = await _http.GetAsync($"api/Math/GetHistory?token={token}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<List<MathCalculation>>(json);

            if (body.Count == 0)
            {
                ViewBag.HistoryMessage = "No history to show";
            }
            
            return View(body);
        }

        ViewBag.HistoryMessage = "No history to show";
        return View();
    }
    
    [
        HttpPost,
        ValidateAntiForgeryToken,
        ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)
    ]
    public async Task<IActionResult> Clear()
    {
        var token = HttpContext.Session.GetString("MathJWT");

        if (token == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        _http.DefaultRequestHeaders.Authorization = new("Bearer", token);
        
        var response = await _http.DeleteAsync($"api/Math/DeleteHistory?token={token}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var body = JsonConvert.DeserializeObject<List<MathCalculation>>(json);

            if (body.Count == 0)
            {
                TempData["ClearMessage"] = "No history to delete";
            }
            else
            {
                TempData["ClearMessage"] = $"Deleted {body.Count} items from history";
            }
        }
        else
        {
            TempData["ClearMessage"] = "Something went wrong";
        }
        
        return RedirectToAction("History");
    }

    private bool ValidateLogin()
    {
        var token = HttpContext.Session.GetString("currentUser");

        return token != null;
    }

    private static List<SelectListItem> GetOperations()
    {
        return
        [
            new() { Value = "1", Text = "+" },
            new() { Value = "2", Text = "-" },
            new() { Value = "3", Text = "*" },
            new() { Value = "4", Text = "/" }
        ];
    }
}
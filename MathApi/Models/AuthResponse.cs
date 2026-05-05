using System.ComponentModel.DataAnnotations;

namespace MathApis.Models;

public class AuthResponse
{
   public string Token { get; set; }
   public string UserId { get; set; }
   
   public AuthResponse(string token, string userId)
   {
      Token = token;
      UserId = userId;
   }
}
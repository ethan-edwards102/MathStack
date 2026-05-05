using System.ComponentModel.DataAnnotations;

namespace MathApis.Models;

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
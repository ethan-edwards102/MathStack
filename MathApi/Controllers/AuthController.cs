using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firebase.Auth;
using MathApis.Models;
using MathApis.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace MathApis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private IConfiguration _config;
    private FirebaseAuthProvider _auth;
    private byte[] _key;

    public AuthController(IConfiguration config)
    {
        _config = config;
        
        _auth = new FirebaseAuthProvider
        (
            new FirebaseConfig(_config["FirebaseMathApp"])
        );

        _key = Encoding.ASCII.GetBytes(_config["MathAppJwtKey"]);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(LoginModel model)
    {
        try
        {
            await _auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password);

            var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
            var currentUserId = fbAuthLink.User.LocalId;
            var currentUserEmail = fbAuthLink.User.Email;

            if (currentUserId != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, currentUserId),
                    new(ClaimTypes.Email, currentUserEmail),
                    new("UserId", currentUserId)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials
                    (
                        new SymmetricSecurityKey(_key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok
                (
                    new AuthResponse(tokenString, currentUserId)
                );
            }
        }
        catch (FirebaseAuthException e)
        {
            var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(e.ResponseData);
            
            AuthLogger.Instance.LogError
            (
                " - Message:" + firebaseEx.Error.Message +
                " - User: " + model.Email +
                " - IP: " + HttpContext.Connection.RemoteIpAddress +
                " - Browser: " + Request.Headers.UserAgent
            );
            
            return Unauthorized($"{firebaseEx.Error.Code} - {firebaseEx.Error.Message}");
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
        
        return BadRequest();
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
            var currentUserId = fbAuthLink.User.LocalId;
            var currentUserEmail = fbAuthLink.User.Email;

            if (currentUserId != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, currentUserId),
                    new(ClaimTypes.Email, currentUserEmail),
                    new("UserId", currentUserId)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials
                    (
                        new SymmetricSecurityKey(_key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok
                (
                    new AuthResponse(tokenString, currentUserId)
                );
            }
        }
        catch (FirebaseAuthException e)
        {
            var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(e.ResponseData);
            
            AuthLogger.Instance.LogError
            (
                " - Message:" + firebaseEx.Error.Message +
                " - User: " + model.Email +
                " - IP: " + HttpContext.Connection.RemoteIpAddress +
                " - Browser: " + Request.Headers.UserAgent
            );
            
            return Unauthorized($"{firebaseEx.Error.Code} - {firebaseEx.Error.Message}");
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
        
        return BadRequest();
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        return Ok(); 
    }
}
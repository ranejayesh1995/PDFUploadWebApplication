using Microsoft.AspNetCore.Mvc;
using PDFUploadWebApplication.Core.DTO;
using PDFUploadWebApplication.Core.Entities;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PDFUploadWebApplication.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

[Route("Auth")]
public class AuthController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(IConfiguration configuration, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }
    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input format." });

        var user = await _userRepository.GetByUsernameAsync(model.Username);
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });
        if (user != null)
        {
            HttpContext.Session.SetString("UserName", user.Username); // ✅ Store in session
        }
        bool validPassword = _passwordHasher.VerifyPasswordHash(model.Password, user.PasswordHash!, user.PasswordSalt!);
        if (!validPassword)
            return Unauthorized(new { message = "Invalid username or password." });

        var token = GenerateJwtToken(user);

        // ✅ Store token in a cookie
        Response.Cookies.Append("JwtToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { token });
    }





    [HttpGet]
    [Route("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Ensure all required fields are provided
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            return View(model);
        }

        // Check if the username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(model.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "Username already taken. Already have an account? Try logging in.");
            return View(model);
        }

        // Secure password hashing
        _passwordHasher.CreatePasswordHash(model.Password, out byte[] hash, out byte[] salt);

        var newUser = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        await _userRepository.AddAsync(newUser);

        return RedirectToAction("Login", "Auth");
    }





    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // ✅ Store user ID in token
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        // ✅ Sign out from authentication system
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // ✅ Clear session completely
        HttpContext.Session.Clear();

        // ✅ Expire JWT token by removing the cookie
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddMinutes(-1), // ✅ Expire instantly upon logout
            HttpOnly = true, // ✅ Secure cookie
            IsEssential = true
        };

        Response.Cookies.Delete("JwtToken"); // ✅ Best way to delete cookies
        Response.Cookies.Append("JwtToken", "", cookieOptions); // ✅ Ensure token is expired

        return RedirectToAction("Login", "Auth"); // ✅ Redirect user to login page
    }
}

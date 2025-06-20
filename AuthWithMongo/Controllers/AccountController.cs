using AuthWithMongo.Data;
using AuthWithMongo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace AuthWithMongo.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly PasswordHasher<User> _hasher;

        public AccountController(MongoDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user, string password)
        {
            var exists = await _context.Users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (exists != null)
            {
                ModelState.AddModelError("", "User already exists");
                return View();
            }

            user.PasswordHash = _hasher.HashPassword(user, password);
            await _context.Users.InsertOneAsync(user);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            if (result == PasswordVerificationResult.Success)
            {
                // TODO: Set a cookie/session here
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid credentials");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }

        //This displays all the users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SecretPage()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return View(users);
        }

    }
}

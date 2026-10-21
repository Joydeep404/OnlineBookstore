using BCrypt.Net;
using OnlineBookstore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OnlineBookstore.Controllers
{
    public class AccountController : Controller
    {
        private OnlineBookstoreContext db = new OnlineBookstoreContext();

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user, string confirmPassword)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
            }
            else if (user.PasswordHash != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Passwords do not match.");
            }

            if (ModelState.IsValid)
            {
                // Hash password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;

                db.Users.Add(user);
                db.SaveChanges();

                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // ✅ CORRECT: Use FirstOrDefault to find by Username (not primary key)
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Successful login
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["FullName"] = user.FullName;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        // POST: Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
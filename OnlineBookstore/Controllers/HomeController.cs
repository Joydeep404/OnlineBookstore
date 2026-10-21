using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineBookstore.Models;
using System.Data.Entity;

namespace OnlineBookstore.Controllers
{
    public class HomeController : Controller
    {
        private OnlineBookstoreContext db = new OnlineBookstoreContext();

        // GET: Home/Index
        public ActionResult Index(int? categoryId, string search)
        {
            // ✅ Allow guests to browse books
            ViewBag.Categories = db.Categories.ToList();

            List<Book> books = db.GetBooksByCategory(categoryId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    b.Author.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            ViewBag.Search = search;
            ViewBag.SelectedCategory = categoryId;
            return View(books);
        }
        // GET: /Home/Welcome
        public ActionResult Welcome()
        {
            return View();
        }
        // GET: Home/Details/5
        public ActionResult Details(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var book = db.Books
                .Include(b => b.Category) // Load category
                .FirstOrDefault(b => b.BookId == id);

            if (book == null)
                return HttpNotFound();

            return View(book);
        }
    }
}
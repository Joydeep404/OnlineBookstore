using OnlineBookstore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace OnlineBookstore.Controllers
{
    public class AdminController : Controller
    {
        private OnlineBookstoreContext db = new OnlineBookstoreContext();

        // 🔐 Helper: Check if current user is admin
        private bool IsAdmin()
        {
            return Session["Username"]?.ToString() == "admin";
        }

        // GET: /Admin
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.BookCount = db.Books.Count();
            ViewBag.OrderCount = db.Orders.Count();
            ViewBag.UserCount = db.Users.Count();
            return View();
        }

        // GET: /Admin/Books
        public ActionResult Books()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var books = db.Books.Include("Category").ToList();
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            return View(books);
        }

        // GET: /Admin/CreateBook
        public ActionResult CreateBook()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            return View();
        }

        // POST: /Admin/CreateBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBook(BookCreateViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                // ✅ Map ViewModel to Book entity
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    // ✅ Handle image upload
                    ImageUrl = "/Content/Images/Books/default.jpg" // default
                };

                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(model.ImageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/Books/"), fileName);
                    model.ImageFile.SaveAs(path);
                    book.ImageUrl = $"/Content/Images/Books/{fileName}";
                }

                db.Books.Add(book);
                db.SaveChanges();
                TempData["Success"] = "Book added successfully!";
                return RedirectToAction("Books");
            }
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            return View(model);
        }

        // GET: /Admin/EditBook/5
        public ActionResult EditBook(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var book = db.Books.Find(id);
            if (book == null) return HttpNotFound();
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            return View(book);
        }

        // POST: /Admin/EditBook/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBook(Book book)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                // ✅ Preserve original CreatedAt value
                var existingBook = db.Books.AsNoTracking().FirstOrDefault(b => b.BookId == book.BookId);
                if (existingBook != null)
                {
                    book.CreatedAt = existingBook.CreatedAt;
                }

                db.Entry(book).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Book updated successfully!";
                return RedirectToAction("Books");
            }
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            return View(book);
        }

        // POST: /Admin/DeleteBook
        [HttpPost]
        public ActionResult DeleteBook(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var book = db.Books.Find(id);
            if (book != null)
            {
                db.Books.Remove(book);
                db.SaveChanges();
                TempData["Success"] = "Book deleted.";
            }
            return RedirectToAction("Books");
        }

        // GET: /Admin/Orders
        public ActionResult Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var orders = db.Orders
                .Include("User")
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        // GET: /Admin/Users
        public ActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var users = db.Users.ToList();
            return View(users);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var user = db.Users.Find(id);
            if (user != null && user.Username != "admin")
            {
                db.Orders.RemoveRange(db.Orders.Where(o => o.UserId == id));
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["Success"] = "User deleted successfully.";
            }
            return RedirectToAction("Users");
        }
        // GET: /Admin/AllOrders
        public ActionResult AllOrders(DateTime? fromDate, DateTime? toDate)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = db.Orders.Include("User").AsQueryable();

            // Apply date filters safely
            if (fromDate.HasValue)
            {
                DateTime fromStart = fromDate.Value.Date; // Start of day
                query = query.Where(o => o.OrderDate >= fromStart);
            }
            if (toDate.HasValue)
            {
                DateTime toEnd = toDate.Value.Date.AddDays(1).AddSeconds(-1); // End of day
                query = query.Where(o => o.OrderDate <= toEnd);
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            // Load order details for view
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var details = db.OrderDetails
                .Where(od => orderIds.Contains(od.OrderId))
                .Join(db.Books, od => od.BookId, b => b.BookId, (od, b) => new OrderItemDetail
                {
                    OrderId = od.OrderId,
                    Quantity = od.Quantity,
                    Title = b.Title,
                    UnitPrice = od.UnitPrice
                })
                .ToList();

            var orderDetailsDict = new Dictionary<string, List<OrderItemDetail>>();
            foreach (var d in details)
            {
                string key = d.OrderId.ToString();
                if (!orderDetailsDict.ContainsKey(key))
                    orderDetailsDict[key] = new List<OrderItemDetail>();
                orderDetailsDict[key].Add(d);
            }

            ViewBag.OrderDetails = orderDetailsDict;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(orders);
        }
        // POST: /Admin/MarkAsShipped
        [HttpPost]
        public ActionResult MarkAsShipped(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var order = db.Orders.Find(id);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Shipped";
                db.SaveChanges();
                TempData["Success"] = "Order marked as shipped!";
            }
            return RedirectToAction("AllOrders");
        }
        // POST: /Admin/CancelOrder
        [HttpPost]
        public ActionResult CancelOrder(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders.Find(id);
            if (order != null && order.Status == "Pending")
            {
                // ✅ Update order status
                order.Status = "Cancelled";

                // ✅ Restore stock for all items in this order
                var orderDetails = db.OrderDetails.Where(od => od.OrderId == id).ToList();
                foreach (var od in orderDetails)
                {
                    var book = db.Books.Find(od.BookId);
                    if (book != null)
                    {
                        book.Stock += od.Quantity;
                    }
                }

                db.SaveChanges();
                TempData["Success"] = "Order #" + id + " has been cancelled and stock restored.";
            }

            return RedirectToAction("AllOrders");
        }
        // GET: /Admin/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return HttpNotFound();

            var orderDetails = db.OrderDetails
                .Where(od => od.OrderId == id)
                .Join(db.Books, od => od.BookId, b => b.BookId, (od, b) => new OrderItemDetail
                {
                    Quantity = od.Quantity,
                    Title = b.Title,
                    UnitPrice = od.UnitPrice
                })
                .ToList();

            ViewBag.OrderDetails = orderDetails;
            return View(order);
        }
    }
}
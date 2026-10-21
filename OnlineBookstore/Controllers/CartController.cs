using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineBookstore.Models;

namespace OnlineBookstore.Controllers
{
    public class CartController : Controller
    {
        private OnlineBookstoreContext db = new OnlineBookstoreContext();

        // Add book to cart
        public ActionResult AddToCart(int bookId, int quantity = 1)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var book = db.Books.Find(bookId);
            if (book == null || book.Stock < quantity)
            {
                TempData["Error"] = "Book not available or insufficient stock.";
                return RedirectToAction("Index", "Home");
            }

            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            var existing = cart.FirstOrDefault(i => i.BookId == bookId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Price = book.Price,
                    Quantity = quantity,
                    ImageUrl = book.ImageUrl ?? "https://via.placeholder.com/150?text=Book"
                });
            }

            Session["Cart"] = cart;
            TempData["Success"] = $"{book.Title} added to cart!";
            return RedirectToAction("Index", "Home");
        }

        // View cart
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            return View(cart);
        }

        // GET: /Cart/CheckoutAddress
        public ActionResult CheckoutAddress()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Cart/CheckoutAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckoutAddress(CheckoutViewModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                Session["DeliveryAddress"] = model;
                return RedirectToAction("ConfirmOrder");
            }

            return View(model);
        }

        // GET: /Cart/ConfirmOrder
        public ActionResult ConfirmOrder()
        {
            if (Session["UserId"] == null || Session["DeliveryAddress"] == null)
                return RedirectToAction("CheckoutAddress");

            var cart = Session["Cart"] as List<CartItem>;
            ViewBag.Total = cart?.Sum(i => i.Price * i.Quantity) ?? 0;
            ViewBag.Address = Session["DeliveryAddress"] as CheckoutViewModel;
            return View(cart);
        }

        // POST: /Cart/FinalizeOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizeOrder()
        {
            if (Session["UserId"] == null || Session["DeliveryAddress"] == null)
                return RedirectToAction("CheckoutAddress");

            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("Index", "Home");
            }

            var userId = (int)Session["UserId"];
            var totalAmount = cart.Sum(i => i.Price * i.Quantity);

            var orderDetails = cart.Select(item => new OrderDetail
            {
                BookId = item.BookId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList();

            try
            {
                var address = Session["DeliveryAddress"] as CheckoutViewModel;

                // ✅ Pass delivery address to PlaceOrder
                db.PlaceOrder(
                    userId,
                    totalAmount,
                    orderDetails,
                    address.FullName,
                    address.PhoneNumber,
                    address.Address,
                    address.City,
                    address.PostalCode
                );

                Session["Cart"] = null;
                Session.Remove("DeliveryAddress");
                TempData["Success"] = "Order placed successfully! Delivery will be made to your address.";
                return RedirectToAction("OrderHistory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to place order. Please try again.";
                return RedirectToAction("CheckoutAddress");
            }
        }

        // Update item quantity
        [HttpPost]
        public ActionResult UpdateQuantity(int bookId, int quantity)
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.BookId == bookId);
                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        var book = db.Books.Find(bookId);
                        if (book != null && quantity <= book.Stock)
                        {
                            item.Quantity = quantity;
                        }
                        else
                        {
                            TempData["Error"] = "Insufficient stock.";
                        }
                    }
                    Session["Cart"] = cart;
                }
            }
            return RedirectToAction("Index");
        }

        // Remove item
        public ActionResult Remove(int bookId)
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart != null)
            {
                cart.RemoveAll(i => i.BookId == bookId);
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/OrderHistory
        public ActionResult OrderHistory()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var userId = (int)Session["UserId"];
            var orders = db.Database.SqlQuery<OrderHistoryItem>(
                "EXEC GetUserOrders @UserId",
                new System.Data.SqlClient.SqlParameter("@UserId", userId)
            ).ToList();

            return View(orders);
        }
    }
}
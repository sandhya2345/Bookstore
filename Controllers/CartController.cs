using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore.Models;
using OnlineBookStore.Services;
using System.Security.Claims;

namespace OnlineBookStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public CartController(
            ApplicationDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cartItems = await _context.ShoppingCart
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] QuantityUpdateRequest request)
        {
            var item = _context.ShoppingCart.FirstOrDefault(c => c.CartId == request.CartId);
            if (item != null)
            {
                item.Quantity = Math.Max(1, item.Quantity + request.Change);
                _context.SaveChanges();
                return Json(new { success = true, newQuantity = item.Quantity });
            }
            return Json(new { success = false });
        }

        public class QuantityUpdateRequest
        {
            public int CartId { get; set; }
            public int Change { get; set; }
        }

        // GET: /Cart/AddToCart/5
        public async Task<IActionResult> AddToCart(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var existingItem = await _context.ShoppingCart
                .FirstOrDefaultAsync(c => c.BookId == id && c.UserId == userId);

            if (existingItem != null)
                existingItem.Quantity++;
            else
                _context.ShoppingCart.Add(new ShoppingCart
                {
                    UserId = userId.Value,
                    BookId = id,
                    Quantity = 1,
                    AddedDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Book added to cart!";
            return RedirectToAction("Browse", "Books");
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(List<int> selectedItems)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Load all cart items for this user
            var cartItems = await _context.ShoppingCart
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Filter by what the user selected
            var selectedCartItems = cartItems
                .Where(ci => selectedItems.Contains(ci.CartId))
                .ToList();

            if (!selectedCartItems.Any())
            {
                TempData["Error"] = "No items selected for checkout.";
                return RedirectToAction("Index");
            }

            // Calculate subtotal and discounts
            var totalBooks = selectedCartItems.Sum(i => i.Quantity);
            var subtotal = selectedCartItems.Sum(i => i.Quantity * i.Book.Price);
            decimal discount = 0m;

            if (totalBooks >= 5)
                discount += subtotal * 0.05m;  // 5% off for 5+ books

            var pastOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == "Completed");
            if (pastOrders >= 10)
                discount += subtotal * 0.10m;  // extra 10% for loyalty

            var finalAmount = subtotal - discount;

            // Build the new Order
            var newOrder = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.UtcNow,
                TotalAmount = subtotal,
                DiscountAmount = discount,
                FinalAmount = finalAmount,
                Status = "Pending",
                ClaimCode = Guid.NewGuid().ToString().Substring(0, 8),
                OrderItems = selectedCartItems.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Book.Price
                }).ToList()
            };

            // Persist to database
            _context.Orders.Add(newOrder);
            _context.ShoppingCart.RemoveRange(selectedCartItems);
            await _context.SaveChangesAsync();

            // Send the claim-code email
            var user = await _context.Users.FindAsync(newOrder.UserId);
            if (user != null)
            {
                await _emailService.SendOrderEmailAsync(
                    user.Email,
                    newOrder.ClaimCode,
                    newOrder.FinalAmount
                );
            }

            TempData["Success"] = "Order placed! Check your email for your claim code.";
            return RedirectToAction("OrderConfirmation", new { orderId = newOrder.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _context.ShoppingCart
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            if (item != null)
            {
                _context.ShoppingCart.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }
            else
            {
                TempData["Error"] = "Item not found in your cart.";
            }

            return RedirectToAction("Index");
        }


        // GET: /Cart/OrderConfirmation
        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return NotFound();
            return View(order);
        }

        // Helper to read the JWT-based user ID
        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : (int?)null;
        }
    }
}

using DAPM2.Models;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DAPM2.Controllers
{
    public class WishlistController : Controller
    {
        // GET: Wishlist
        WebStory2Entities1 db = new WebStory2Entities1();

        [HttpPost]
        public JsonResult AddToWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào danh sách theo dõi." });
            }

            using (var context = new WebStory2Entities1())
            {
                var wishlistItem = context.Wishlists.FirstOrDefault(w => w.UserID == userId && w.ProductID == productId);
                if (wishlistItem != null)
                {
                    // Nếu sản phẩm đã tồn tại, xóa khỏi danh sách theo dõi
                    context.Wishlists.Remove(wishlistItem);
                    context.SaveChanges();
                    return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi danh sách theo dõi.", reload = true });
                }
                else
                {
                    // Nếu sản phẩm chưa tồn tại, thêm vào danh sách theo dõi
                    wishlistItem = new Wishlist
                    {
                        UserID = userId.Value,
                        ProductID = productId,
                        DateAdded = DateTime.Now
                    };
                    context.Wishlists.Add(wishlistItem);
                    context.SaveChanges();
                    return Json(new { success = true, message = "Sản phẩm đã được thêm vào danh sách theo dõi.", reload = true });
                }
            }
        }



        [HttpPost]
        public JsonResult RemoveFromWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để xóa khỏi danh sách theo dõi." });
            }

            using (var context = new WebStory2Entities1())
            {
                var wishlistItem = context.Wishlists.FirstOrDefault(w => w.UserID == userId && w.ProductID == productId);
                if (wishlistItem != null)
                {
                    context.Wishlists.Remove(wishlistItem);
                    context.SaveChanges();
                }
            }

            return Json(new { success = true });
        }



        private const int PageSize = 15;

        public ActionResult Wishlist(int page = 1)
        {
            int? userId = GetCurrentUserId(); // Lấy ID người dùng hiện tại

            if (!userId.HasValue)
            {
                // Chuyển đến trang đăng nhập nếu người dùng chưa đăng nhập
                return RedirectToAction("Login", "Login");
            }

            // Lấy danh sách sản phẩm từ wishlist theo `UserID`
            var userWishlistProducts = db.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserID == userId.Value)
                .Select(w => w.Product)
                .Distinct()
                .ToList();

            // Tính tổng số lượng sản phẩm và phân trang
            int totalStories = userWishlistProducts.Count;
            var pagedStories = userWishlistProducts
                .OrderBy(s => s.ProductID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Chuẩn bị ViewModel để truyền dữ liệu sang View
            var viewModel = new WishlistViewModel
            {
                Products = pagedStories,
                Categories = CategoryService.GetAllCategories(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalStories / PageSize)
            };

            return View(viewModel);
        }


        private int? GetCurrentUserId()
        {
            using (var context = new WebStory2Entities1())
            {
                // Lấy thông tin người dùng từ Session
                var fullname = Session["FullName"]?.ToString();

                if (string.IsNullOrEmpty(fullname))
                {
                    return null; // Trả về null nếu không có người dùng đăng nhập
                }

                // Tìm người dùng trong cơ sở dữ liệu
                var user = context.Users.FirstOrDefault(u => u.FullName == fullname);

                if (user == null)
                {
                    return null; // Trả về null nếu không tìm thấy người dùng
                }

                return user.UserID; // Trả về UserID nếu người dùng tồn tại
            }
        }
    }
}
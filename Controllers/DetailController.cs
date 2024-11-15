using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAPM2.Models;
using System.Data.Entity;
using System.Security.Claims;

namespace DAPM2.Controllers
{
    public class DetailController : Controller
    {
        // GET: Detail
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(int id)
        {
            using (var database = new WebStory2Entities1())
            {
                // Lấy sản phẩm từ dịch vụ
                var product = ProductService.GetProductById(id);
                if (product == null)
                {
                    return HttpNotFound(); // Xử lý nếu không tìm thấy sản phẩm
                }

                // Lấy danh sách chương từ dịch vụ
                var chapters = ChapterService.GetChaptersByProductId(id);

                // Tải sản phẩm kèm danh sách bình luận và thông tin người dùng
                var productWithReviews = database.Products
                    .Include(p => p.UserReviews.Select(r => r.User))
                    .Where(p => p.ProductID == id)
                    .ToList()
                    .FirstOrDefault();

                if (productWithReviews == null)
                {
                    return HttpNotFound(); // Xử lý nếu không tìm thấy sản phẩm với bình luận
                }

                // Đếm số lượng người theo dõi
                int followerCount = GetFollowerCount(id);
                ViewBag.FollowerCount = followerCount;

                // Truyền tên danh mục vào ViewBag
                ViewBag.CategoryName = product.Category?.CategoryName;

                // Hiển thị thông báo thành công hoặc lỗi nếu có
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

                // Chuẩn bị ViewModel để truyền dữ liệu sang View
                var model = new ProductDetailViewModel
                {
                    Product = product,
                    Category = product.Category,
                    Chapters = chapters,
                    UserReviews = productWithReviews.UserReviews.ToList(),
                    Categories = CategoryService.GetAllCategories(),
                };

                var userId = GetCurrentUserId(); // Giả sử bạn có phương thức để lấy user ID
                if (userId != null)
                {
                    model.IsInWishlist = IsProductInWishlist(userId.Value, model.Product.ProductID);
                }

                return View(model);
            }
        }
        // Phương thức để kiểm tra sản phẩm trong wishlist
        private bool IsProductInWishlist(int userId, int productId)
        {
            using (var context = new WebStory2Entities1())
            {
                return context.Wishlists.Any(w => w.UserID == userId && w.ProductID == productId);
            }
        }
        public int GetFollowerCount(int productId)
        {
            using (var context = new WebStory2Entities1())
            {
                // Đếm số lượng bản ghi trong bảng Wishlist mà có ProductID tương ứng
                return context.Wishlists.Count(w => w.ProductID == productId);
            }
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
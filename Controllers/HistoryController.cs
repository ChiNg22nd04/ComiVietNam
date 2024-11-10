using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class HistoryController : Controller
    {
        WebStory2Entities1 database = new WebStory2Entities1();

        // GET: History


        private const int PageSize = 15; // Số truyện trên mỗi trang
        public ActionResult HistoryReading(int page = 1)
        {
            int? userId = GetCurrentUserId(); // Lấy ID người dùng hiện tại

            if (!userId.HasValue)
            {
                // Chuyển đến trang đăng nhập nếu người dùng chưa đăng nhập
                return RedirectToAction("Login", "Login");
            }

            // Lấy danh sách sản phẩm từ lịch sử đọc theo `UserID`
            var userHistoryProducts = database.ReadingHistories
                .Include(h => h.Product)
                .Where(h => h.UserID == userId.Value)
                .Select(h => h.Product)
                .Distinct()
                .ToList();

            // Tính tổng số lượng sản phẩm và phân trang
            int totalStories = userHistoryProducts.Count;
            var pagedStories = userHistoryProducts
                .OrderBy(s => s.ProductID)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Chuẩn bị ViewModel để truyền dữ liệu sang View
            var viewModel = new HistoryViewModel
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
            // Lấy thông tin người dùng từ Session
            var fullname = Session["FullName"]?.ToString();

            if (string.IsNullOrEmpty(fullname))
            {
                return null; // Trả về null nếu không có người dùng đăng nhập
            }

            // Tìm người dùng trong cơ sở dữ liệu
            var user = database.Users.FirstOrDefault(u => u.FullName == fullname);

            if (user == null)
            {
                return null; // Trả về null nếu không tìm thấy người dùng
            }

            return user.UserID; // Trả về UserID nếu người dùng tồn tại
        }
    }
}

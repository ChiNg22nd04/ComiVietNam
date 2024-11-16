using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class AllStoryController : Controller
    {
        // GET: AllStory
        WebStory2Entities1 database = new WebStory2Entities1();
        private const int PageSize = 12;
        public ActionResult AllStory(int categoryID, int page = 1)
        {
            // Initialize view model
            // lấy danh mục
            var categories = database.Categories.ToList();
            var products = database.Products.ToList(); //Lấy danh sách tất cả các truyện
            //Lấy thông tin chi tiết của danh mục cụ thể dựa vào categoryID truyền vào.
            var category = CategoryService.GetCategoryById(categoryID);
            //Gán tên của danh mục vào ViewBag.CategoryName để có thể hiển thị trên view.
            //Dấu ? đảm bảo không lỗi khi category có thể null.
            ViewBag.CategoryName = category?.CategoryName;

            // Lấy tổng số truyện trong danh mục này
            int totalStories = products.Count();

            // Áp dụng phân trang cho truy vấn
            var pagedStories = products
                .OrderBy(s => s.ProductID) // Sắp xếp truyện nếu cần
                .Skip((page - 1) * PageSize) // Bỏ qua các truyện của các trang trước
                .Take(PageSize) // Lấy số truyện cho trang hiện tại
                .ToList();

            var viewModel = new CategoryViewModel
            {
                categories = categories,
                products = products,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalStories / PageSize) // Tính tổng số trang
            };

            // Pass the model to the view
            return View(viewModel);
        }
        public ActionResult SearchBooksByTitle(string searchTerm)
        {
            // Gọi hàm tìm kiếm từ ProductService
            var books = ProductService.SearchBooksByTitle(searchTerm);

            // Lưu trữ danh sách sách tìm được vào TempData để chuyển qua Controller khác
            TempData["SearchResults"] = books;

            // Chuyển hướng tới Action Search của ProductController
            return RedirectToAction("SearchBooksByTitle", "Home", new { searchTerm });
        }

    }
}
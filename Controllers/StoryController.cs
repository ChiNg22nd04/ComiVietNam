using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class StoryController : Controller
    {
        // GET: Story
        public ActionResult Index()
        {
            return View();
        }
        private const int PageSize = 15; // Số truyện trên mỗi trang
        public ActionResult ListStory(int categoryID, int page = 1)
        {
            // Sử dụng ProductService để lấy danh sách truyện theo thể loại
            var stories = ProductService.GetStoriesByCategory(categoryID);

            // Lấy tổng số truyện trong danh mục này
            int totalStories = stories.Count();

            // Áp dụng phân trang cho truy vấn
            var pagedStories = stories
                .OrderBy(s => s.ProductID) // Sắp xếp truyện nếu cần
                .Skip((page - 1) * PageSize) // Bỏ qua các truyện của các trang trước
                .Take(PageSize) // Lấy số truyện cho trang hiện tại
                .ToList();

            // Lấy tên thể loại dựa trên categoryID để hiển thị trên trang
            var category = CategoryService.GetCategoryById(categoryID);
            ViewBag.CategoryName = category?.CategoryName;
            ViewBag.CategoryID = categoryID; // Gán categoryID vào ViewBag

            // Truyền dữ liệu truyện vào view thông qua ViewModel
            var viewModel = new StoryViewModel
            {
                Products = pagedStories,
                Categories = CategoryService.GetAllCategories(), // Tất cả danh mục
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalStories / PageSize) // Tính tổng số trang
            };

            return View(viewModel);
        }

    }
}
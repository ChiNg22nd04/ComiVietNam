using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo đối tượng kết nối cơ sở dữ liệu
        WebStory2Entities1 database = new WebStory2Entities1();

        // Phương thức xử lý cho trang chủ
        public ActionResult Index()
        {
            // Lấy danh sách tất cả các danh mục từ cơ sở dữ liệu
            var categories = database.Categories.ToList();

            // Lấy danh sách tất cả sản phẩm (stories) từ cơ sở dữ liệu
            var products = database.Products.ToList();

            // Lấy danh sách sản phẩm nổi bật (có ViewCount > 0), sắp xếp giảm dần theo ViewCount
            var featuredStories = database.Products
                                          .Where(p => p.ViewCount > 0)
                                          .OrderByDescending(p => p.ViewCount)
                                          .ToList();

            // Lấy danh sách sản phẩm được xem nhiều nhất, sắp xếp giảm dần theo ViewCount
            var mostViewedStories = database.Products
                                .OrderByDescending(p => p.ViewCount)
                                .ToList();

            // Lấy danh sách sản phẩm mới được thêm, sắp xếp giảm dần theo CreatedAt
            var recentlyAddedStories = database.Products
                                               .OrderByDescending(p => p.CreatedAt)
                                               .ToList();

            // Lấy danh sách sản phẩm mới được cập nhật, sắp xếp theo ngày cập nhật mới nhất
            var recentlyUpdatedStories = database.UpdateChapters
                                                  .GroupBy(uc => uc.ProductID) // Gom nhóm theo ProductID
                                                  .Select(g => g.OrderByDescending(uc => uc.DateAdded).FirstOrDefault()) // Lấy bản ghi mới nhất trong nhóm
                                                  .Where(uc => uc != null) // Loại bỏ các bản ghi null
                                                  .Select(uc => uc.Product) // Lấy sản phẩm tương ứng
                                                  .OrderByDescending(p => p.UpdateChapters.Max(uc => uc.DateAdded)) // Sắp xếp giảm dần theo ngày cập nhật mới nhất
                                                  .ToList();

            // Tạo view model để truyền dữ liệu vào view
            var viewModel = new CategoryViewModel
            {
                categories = categories, // Danh sách danh mục
                products = featuredStories, // Danh sách sản phẩm nổi bật
                recentlyAddedStories = recentlyAddedStories, // Sản phẩm mới thêm
                mostViewedStories = mostViewedStories, // Sản phẩm được xem nhiều nhất
                recentlyUpdatedStories = recentlyUpdatedStories // Sản phẩm được cập nhật gần đây
            };

            // Trả về view cùng với view model
            return View(viewModel);
        }

        // Phương thức hiển thị danh sách stories theo danh mục
        public ActionResult CategoryStories(int categoryID)
        {
            // Lấy danh sách stories theo CategoryID
            var stories = database.Products.Where(s => s.CategoryID == categoryID).ToList();

            // Trả về view cùng với danh sách stories
            return View(stories);
        }

        // Phương thức thực hiện live search (tìm kiếm theo thời gian thực)
        public ActionResult LiveSearch(string searchTerm)
        {
            // Tìm kiếm sản phẩm theo từ khóa trong Title
            var stories = database.Products
                            .Where(s => s.Title.Contains(searchTerm)) // Tìm theo Title
                            .Select(s => new { s.Title, s.ProductID, ImagePath = s.ImageProcductURL }) // Chọn các trường cần thiết
                            .ToList();

            // Trả về kết quả dưới dạng JSON để dùng cho AJAX
            return Json(stories, JsonRequestBehavior.AllowGet);
        }

        // Phương thức tìm kiếm sách theo tiêu đề
        public ActionResult SearchBooksByTitle(string searchTerm)
        {
            // Gọi service để tìm kiếm sách theo Title
            var books = ProductService.SearchBooksByTitle(searchTerm);

            // Trả về view "SearchBooksByTitle" cùng với danh sách sách tìm được
            return View("SearchBooksByTitle", books);
        }
    }

}
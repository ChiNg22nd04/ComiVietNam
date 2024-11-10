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
        WebStory2Entities1 database = new WebStory2Entities1();

        public ActionResult Index()
        {
            // Lấy các thể loại từ cơ sở dữ liệu
            var categories = database.Categories.ToList();
            var products = database.Products.ToList(); // Lấy danh sách sản phẩm


            // Lấy danh sách sản phẩm sắp xếp theo lượt xem giảm dần
            // Lấy danh sách sản phẩm có lượt xem > 0 và sắp xếp theo lượt xem giảm dần
            var featuredStories = database.Products
                                          .Where(p => p.ViewCount > 0) // Chỉ lấy các sản phẩm có lượt xem
                                          .OrderByDescending(p => p.ViewCount)
                                          .ToList();
            var mostViewedStories = database.Products
                                .OrderByDescending(p => p.ViewCount)
                                .ToList();

            // Lấy danh sách truyện mới được thêm, sắp xếp theo thời gian tạo giảm dần
            var recentlyAddedStories = database.Products
                                               .OrderByDescending(p => p.CreatedAt)
                                               .ToList();

            // Lấy danh sách truyện đã được cập nhật chương mới nhất
            var recentlyUpdatedStories = database.UpdateChapters
                                                  .GroupBy(uc => uc.ProductID)
                                                  .Select(g => g.OrderByDescending(uc => uc.DateAdded).FirstOrDefault())
                                                  .Where(uc => uc != null)
                                                  .Select(uc => uc.Product)
                                                  .OrderByDescending(p => p.UpdateChapters.Max(uc => uc.DateAdded)) // Sắp xếp theo ngày cập nhật mới nhất
                                                  .ToList();

            // Truyền các thể loại vào view model
            var viewModel = new CategoryViewModel
            {
                categories = categories,
                products = featuredStories, // Chỉ nếu cần thiết, nếu không chỉ lấy sản phẩm cho tìm kiếm
                recentlyAddedStories = recentlyAddedStories,
                mostViewedStories = mostViewedStories,
                recentlyUpdatedStories = recentlyUpdatedStories
            };

            return View(viewModel);
        }
        public ActionResult CategoryStories(int categoryID)
        {
            // Lấy các câu chuyện dựa trên ID thể loại
            var stories = database.Products.Where(s => s.CategoryID == categoryID).ToList();

            // Truyền các câu chuyện vào view
            return View(stories);
        }

    }
}
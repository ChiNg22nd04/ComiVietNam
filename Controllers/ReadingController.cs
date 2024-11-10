using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DAPM2.Models;
using System.Data.Entity;
using System.IO;
using System.Web.UI.WebControls;
using System.Data.Entity.Infrastructure;

namespace DAPM2.Controllers
{
    public class ReadingController : Controller
    {
        WebStory2Entities1 database = new WebStory2Entities1();
        // GET: Reading
        public ActionResult ReadingChapter(int id, int chapterId)
        {
            ProductService.IncreaseViewCount(id); // Tăng lượt xem

            using (var context = new WebStory2Entities1())
            {
                var product = ProductService.GetProductById(id);

                var currentChapter = context.UpdateChapters
                    .Include(ch => ch.ChapterImages)
                    .FirstOrDefault(ch => ch.ChapterID == chapterId && ch.ProductID == id);

                if (product == null || currentChapter == null)
                {
                    return HttpNotFound();
                }

                // Lấy chương trước và chương sau
                var previousChapter = context.UpdateChapters
                    .Where(ch => ch.ProductID == id && ch.ChapterID < currentChapter.ChapterID)
                    .OrderByDescending(ch => ch.ChapterID)
                    .FirstOrDefault();

                var nextChapter = context.UpdateChapters
                    .Where(ch => ch.ProductID == id && ch.ChapterID > currentChapter.ChapterID)
                    .OrderBy(ch => ch.ChapterID)
                    .FirstOrDefault();

                var model = new ImageDetailViewModel
                {
                    Product = product,
                    Category = product.Category,
                    CurrentChapter = currentChapter,
                    Chapters = new List<UpdateChapter> { currentChapter },
                    Categories = CategoryService.GetAllCategories(),
                    PreviousChapter = previousChapter,
                    NextChapter = nextChapter
                };

                int? userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    SaveReadingHistory(userId.Value, product.ProductID, chapterId);
                }

                return View(model);
            }
        }

        public ActionResult DeleteImage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteImage(int id, int? chapterId, int productId)
        {
            // Tìm hình ảnh để xóa theo ID
            var image = database.ChapterImages.Find(id); // Sử dụng 'id' để tìm hình ảnh

            if (image == null)
            {
                // Nếu hình ảnh không tồn tại, hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Hình ảnh không tồn tại.";
                return RedirectToAction("ReadingChapter", "Reading", new { id = productId, chapterId = chapterId });
            }

            // Xóa hình ảnh và lưu thay đổi
            database.ChapterImages.Remove(image);
            database.SaveChanges();

            // Chuyển hướng về trang chi tiết sản phẩm sau khi xóa
            TempData["SuccessMessage"] = "Hình ảnh đã được xóa thành công."; // Thông báo thành công tùy chọn
            return RedirectToAction("ReadingChapter", "Reading", new { id = productId, chapterId = chapterId });
        }

        public ActionResult EditImage(int id, int? chapterId, int productId)
        {
            var image = database.ChapterImages.Find(id);
            if (image == null)
            {
                TempData["ErrorMessage"] = "Hình ảnh không tồn tại.";
                return RedirectToAction("ReadingChapter", "Reading", new { id = productId, chapterId = chapterId });
            }

            return View(image); // Pass the existing image model to the view.
        }


        [HttpPost]
        public ActionResult EditImage(ChapterImage image, int? chapterId, int productId, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    try
                    {
                        // Lấy tên file và đảm bảo không trùng tên
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/images/chapters"), fileName);

                        // Lưu file vào thư mục
                        imageFile.SaveAs(path);

                        // Tìm ảnh hiện tại trong CSDL
                        var existingImage = database.ChapterImages.Find(image.ImageID);
                        if (existingImage == null)
                        {
                            ModelState.AddModelError("", "Không tìm thấy hình ảnh.");
                            return View(image);
                        }

                        // Cập nhật URL và thứ tự ảnh
                        existingImage.ImageURL = "/Content/images/chapters/" + fileName;
                        existingImage.ImageOrder = image.ImageOrder;

                        // Lưu thay đổi vào CSDL
                        database.SaveChanges();

                        // Điều hướng về trang đọc chương
                        return RedirectToAction("ReadingChapter", "Reading", new { id = productId, chapterId = chapterId });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi khi lưu hình: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Vui lòng chọn một hình ảnh hợp lệ.");
                }
            }

            // Trả về view nếu có lỗi
            return View(image);
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


        public static void SaveReadingHistory(int userID, int productID, int chapterID)
        {
            using (var db = new WebStory2Entities1())
            {
                // Kiểm tra xem lịch sử đọc cho chương này đã tồn tại chưa
                var existingHistory = db.ReadingHistories
                                        .FirstOrDefault(h => h.UserID == userID && h.ProductID == productID && h.ChapterID == chapterID);

                if (existingHistory == null) // Chỉ lưu nếu chưa có lịch sử cho chương này
                {
                    var history = new ReadingHistory
                    {
                        UserID = userID,
                        ProductID = productID,
                        ChapterID = chapterID,
                        LastReadDate = DateTime.Now
                    };

                    db.ReadingHistories.Add(history);
                    db.SaveChanges();
                }
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAPM2.Models;
using System.Data.Entity; // Đảm bảo đã bao gồm thư viện này
using System.IO;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace DAPM2.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        WebStory2Entities1 database = new WebStory2Entities1();
        // Trang Dashboard
        public ActionResult Dashboard()
        {
            if (Session["RoleUser"] == null || Session["RoleUser"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        // Trang Quản Lý Người Dùng
        public ActionResult ManageUsers()
        {
            // Lấy danh sách người dùng từ database và truyền vào View
            var users = GetUsersFromDatabase(); // Hàm giả định để lấy dữ liệu người dùng
            var model = new ManageUsersViewModel
            {
                Users = users
            };
            return View(model);
        }

        // Hàm giả định để lấy dữ liệu người dùng từ database
        private List<User> GetUsersFromDatabase()
        {
            // Thay thế bằng code lấy danh sách người dùng từ cơ sở dữ liệu của bạn
            var users = database.Users.ToList();
            return users;
        }
        public ActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại hay chưa
                var check = database.Users.FirstOrDefault(u => u.Email == user.Email);
                if (check != null)
                {
                    ViewBag.error = "Email đã tồn tại.";
                    return View();
                }

                // Kiểm tra mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số
                if (!IsValidPassword(user.PasswordHash))
                {
                    ViewBag.error = "Mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số.";
                    return View();
                }

                var newUser = new User
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash, // Đảm bảo bạn mã hóa mật khẩu
                    RoleUser = user.RoleUser, // Gán vai trò
                    RegisteredDate = DateTime.Now
                };

                database.Users.Add(newUser);
                database.SaveChanges();
                TempData["SuccessMessage"] = "Thêm tài khoản thành công.";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.error = "Lỗi";
            return View();
        }
        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }
        public ActionResult DeleteUser()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteUser(int id)
        {
            var user = database.Users.Find(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("ManageUsers");
            }

            database.Users.Remove(user);
            database.SaveChanges();

            // Chuyển hướng về trang quản lý người dùng sau khi xóa
            return RedirectToAction("ManageUsers");
        }
        public ActionResult EditUser()
        {
            return View();
        }

        [HttpGet]
        public ActionResult EditUser(int id)
        {
            var user = database.Users.Find(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("ManageUsers");
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = database.Users.Find(user.UserID);
                if (existingUser == null)
                {
                    ViewBag.error = "Tài khoản không tồn tại.";
                    return RedirectToAction("ManageUsers");
                }

                // Kiểm tra email đã tồn tại hay chưa
                var check = database.Users.FirstOrDefault(u => u.Email == user.Email);
                if (check == null)
                {
                    ViewBag.error = "Email đã tồn tại.";
                    return View();
                }

                // Kiểm tra mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số
                if (!IsValidPassword(user.PasswordHash))
                {
                    ViewBag.error = "Mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số.";
                    return View();
                }

                // Cập nhật các thuộc tính
                existingUser.FullName = user.FullName;
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.RoleUser = user.RoleUser;
                database.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction("ManageUsers");
            }

            return View();
        }



        // Trang Quản Lý Truyện
        public ActionResult ManageStories()
        {
            var stories = GetAllStories();
            var categories = database.Categories.ToList(); // Lấy danh sách thể loại

            var model = new StoryViewModel
            {
                Products = stories,
                Categories = categories // Truyền danh sách thể loại
            };
            return View(model);
        }

        private List<Product> GetAllStories() // Đảm bảo kiểu trả về là List<Story>
        {
            // Lấy danh sách truyện cùng với thể loại từ cơ sở dữ liệu
            var stories = database.Products.Include(p => p.Category).ToList(); // Thay "Include("Category")" bằng lambda
            return stories;
        }


        private List<UserReview> UserReviews() // Đảm bảo kiểu trả về là List<Story>
        {
            // Lấy danh sách truyện cùng với thể loại từ cơ sở dữ liệu
            var data = database.UserReviews.Include(p => p.Comment).ToList(); // Thay "Include("Category")" bằng lambda
            return data;
        }

        public ActionResult AddStory()
        {
            // Lấy danh sách các thể loại từ database
            var categories = database.Categories.ToList();

            // Truyền danh sách này vào ViewBag
            ViewBag.categories = new SelectList(categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        public ActionResult AddStory(Product product, int? productId, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu file không null và là ảnh
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/chapters"), fileName);
                    imageFile.SaveAs(path);

                    var newProduct = new Product
                    {
                        Title = product.Title,
                        Author = product.Author,
                        PublicationYear = product.PublicationYear,
                        CategoryID = product.CategoryID,
                        Content = product.Content,
                        ImageProcductURL = "/Content/images/chapters/" + fileName,
                        CreatedAt = DateTime.Now
                    };
                    database.Products.Add(newProduct);
                    database.SaveChanges();
                    return RedirectToAction("ManageStories");
                }
                else
                {
                    // Nếu không có ảnh hoặc không hợp lệ, hiển thị thông báo lỗi
                    ViewBag.Error = "Vui lòng chọn một file ảnh hợp lệ.";
                }
            }
            // Truyền lại danh sách thể loại nếu có lỗi
            ViewBag.categories = new SelectList(database.Categories.ToList(), "CategoryID", "CategoryName");
            return View();
        }


        public ActionResult DeleteStory()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteStory(int id)
        {
            var stories = database.Products.Find(id);
            if (stories == null)
            {
                TempData["ErrorMessage"] = "Truyện không tồn tại.";
                return RedirectToAction("ManageStories");
            }

            database.Products.Remove(stories);
            database.SaveChanges();

            // Chuyển hướng về trang quản lý người dùng sau khi xóa
            return RedirectToAction("ManageStories");
        }

        public ActionResult EditStory(int id)
        {
            var existingProduct = database.Products.Find(id);
            if (existingProduct == null)
            {
                TempData["ErrorMessage"] = "Truyện không tồn tại.";
                return RedirectToAction("ManageStories");
            }

            // Lấy danh sách các category từ database
            var categories = database.Categories.ToList();

            // Tạo một view model hoặc sử dụng Product để truyền dữ liệu
            var model = existingProduct;

            // Truyền danh sách categories vào ViewBag
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", existingProduct.CategoryID);

            return View(model);
        }

        [HttpPost]
        public ActionResult EditStory(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = database.Products.Find(product.ProductID);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Truyện không tồn tại.";
                    return RedirectToAction("ManageStories");
                }

                // Cập nhật các thuộc tính
                existingProduct.Title = product.Title;
                existingProduct.Author = product.Author;
                existingProduct.PublicationYear = product.PublicationYear;
                existingProduct.CategoryID = product.CategoryID; // Đảm bảo cập nhật CategoryID
                existingProduct.Content = product.Content;

                database.SaveChanges();
                return RedirectToAction("ManageStories");
            }

            // Nếu model không hợp lệ, lấy lại danh sách categories để hiển thị trong dropdown
            var categories = database.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName", product.CategoryID);

            return View(product);
        }


        // Trang Quản Lý Thể Loại
        public ActionResult ManageCategories()
        {
            // Lấy danh sách truyện từ cơ sở dữ liệu
            var categories = GetAllCategory();
            var model = new CategoryViewModel
            {
                categories = categories // Đảm bảo sử dụng Stories thay vì Products
            };
            return View(model);
        }
        private List<Category> GetAllCategory() // Đảm bảo kiểu trả về là List<Story>
        {
            // Thay thế bằng code lấy danh sách truyện từ cơ sở dữ liệu của bạn
            var categories = database.Categories.ToList(); // Thay đổi thành bảng truyện
            return categories;
        }

        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại hay chưa
                var check = database.Categories.FirstOrDefault(u => u.CategoryName == category.CategoryName);
                if (check == null)
                {
                    var newCategory = new Category
                    {
                        CategoryName = category.CategoryName,
                    };

                    database.Categories.Add(newCategory);
                    database.SaveChanges();

                    return RedirectToAction("ManageCategories");
                }
                else
                {
                    ViewBag.Error = "Thể loại đã tồn tại.";
                }
            }

            return View(category);
        }
        public ActionResult DeleteCategory()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            var category = database.Categories.Find(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Thể loại không tồn tại.";
                return RedirectToAction("ManageCatories");
            }

            database.Categories.Remove(category);
            database.SaveChanges();

            // Chuyển hướng về trang quản lý người dùng sau khi xóa
            return RedirectToAction("ManageCategories");
        }

        public ActionResult EditCategory(int id)
        {
            var category = database.Categories.Find(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Thể loại không tồn tại.";
                return RedirectToAction("ManageCategories");
            }

            return View(category);
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = database.Categories.Find(category.CategoryID);
                if (existingCategory == null)
                {
                    TempData["ErrorMessage"] = "Thể loại không tồn tại.";
                    return RedirectToAction("ManageCategories");
                }

                // Cập nhật các thuộc tính
                existingCategory.CategoryName = category.CategoryName;
                database.SaveChanges();
                return RedirectToAction("ManageCategories");
            }

            return View(category);
        }


        // Trang Đăng Xuất
        public ActionResult Logout()
        {
            Session["AdminUsername"] = null;
            return RedirectToAction("Login", "Admin");
        }


        public ActionResult UploadChapter(int productId)
        {
            ViewBag.ProductID = productId; // Lưu productID vào ViewBag
            return View();
        }
        [HttpPost]
        public ActionResult UploadChapter(int productId, Product product, UpdateChapter updateChapter)
        {
            if (ModelState.IsValid)
            {
                // Tạo chương mới
                var newChapter = new UpdateChapter
                {
                    ChapterID = updateChapter.ChapterID,
                    ProductID = productId,
                    EpisodeChapterName = updateChapter.EpisodeChapterName,
                    DateAdded = DateTime.Now
                };
                database.UpdateChapters.Add(newChapter);
                database.SaveChanges();

                // Chuyển hướng sang ImageStory với chapterID và productID
                return RedirectToAction("Detail", "Detail", new { id = productId });
            }
            ViewBag.ProductID = productId;
            ViewBag.categories = new SelectList(database.Categories.ToList(), "CategoryID", "CategoryName");
            return View(product);
        }



        public ActionResult ImageStory(int chapterId, int productId)
        {
            ViewBag.ChapterID = chapterId;
            ViewBag.ProductID = productId;
            // Load other necessary data, if any, before returning the view
            return View();
        }

        // POST: Admin/ImageStory

        [HttpPost]
        public ActionResult ImageStory(int chapterId, ChapterImage chapterImage, int? productId, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Log giá trị của chapterId và chapterImage.ImageOrder
                Debug.WriteLine($"chapterId: {chapterId}");
                Debug.WriteLine($"ImageOrder: {chapterImage.ImageOrder}");

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Log thông tin file ảnh
                    Debug.WriteLine($"File Name: {imageFile.FileName}");
                    Debug.WriteLine($"File Size: {imageFile.ContentLength}");

                    // Kiểm tra trùng ImageOrder trong cùng ChapterID
                    var existingImage = database.ChapterImages
                        .FirstOrDefault(ci => ci.ChapterID == chapterId && ci.ImageOrder == chapterImage.ImageOrder);

                    if (existingImage != null)
                    {
                        ViewBag.Error = "Thứ tự ảnh đã tồn tại. Vui lòng chọn thứ tự khác.";
                        // Log lỗi nếu thứ tự ảnh đã tồn tại
                        Debug.WriteLine("Error: Thứ tự ảnh đã tồn tại.");
                    }
                    else
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/images/chapters"), fileName);
                        imageFile.SaveAs(path);

                        var newImage = new ChapterImage
                        {
                            ImageID = chapterImage.ImageID,
                            ChapterID = chapterId,
                            ImageURL = "/Content/images/chapters/" + fileName,
                            ImageOrder = chapterImage.ImageOrder,
                            UploadedDate = DateTime.Now
                        };

                        // Log dữ liệu ChapterImage mới
                        Debug.WriteLine($"New Image: {newImage.ImageID}, {newImage.ChapterID}, {newImage.ImageURL}, {newImage.ImageOrder}, {newImage.UploadedDate}");

                        database.ChapterImages.Add(newImage);
                        database.SaveChanges();

                        // Kiểm tra productId có tồn tại
                        if (productId.HasValue)
                        {
                            Debug.WriteLine($"Redirecting to Detail for productId: {productId.Value}");
                            return RedirectToAction("Detail", "Detail", new { id = productId });
                        }
                        else
                        {
                            Debug.WriteLine("Redirecting to Index as no productId");
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    ViewBag.Error = "Vui lòng chọn một file ảnh hợp lệ.";
                    // Log lỗi nếu không có file ảnh
                    Debug.WriteLine("Error: No valid image file provided.");
                }
            }

            // Giữ lại chapterId và productId để hiển thị lại form nếu có lỗi
            ViewBag.ChapterID = chapterId;
            ViewBag.ProductID = productId;

            // Log chapterId và productId khi có lỗi
            Debug.WriteLine($"Error occurred. chapterId: {chapterId}, productId: {productId}");

            return View();
        }


        public ActionResult DeleteChapter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteChapter(int id, int productId)
        {
            // Find the chapter to delete by its id
            var chapter = database.UpdateChapters.Find(id);

            if (chapter == null)
            {
                // If the chapter is not found, display an error message
                TempData["ErrorMessage"] = "Chương không tồn tại.";
                return RedirectToAction("Detail", "Detail", new { id = productId });
            }

            // Remove the chapter and save changes
            database.UpdateChapters.Remove(chapter);
            database.SaveChanges();

            // Redirect to the detail page of the product after deletion
            return RedirectToAction("Detail", "Detail", new { id = productId });
        }


        public ActionResult EditChapter(int chapterId, int productId)
        {
            var chapter = database.UpdateChapters.Find(chapterId);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = "Chapter không tồn tại.";
                return RedirectToAction("Detail", "Detail", new { id = productId });
            }

            // You might want to map the chapter to a ViewModel if necessary.
            return View(chapter);
        }

        [HttpPost]
        public ActionResult EditChapter(UpdateChapter chapter)
        {
            if (ModelState.IsValid)
            {
                var existingChapter = database.UpdateChapters.Find(chapter.ChapterID);
                if (existingChapter == null)
                {
                    ModelState.AddModelError("", "Chapter không tồn tại.");
                    return View(chapter);  // Return the view with the current input data
                }

                // Update chapter properties
                existingChapter.ProductID = chapter.ProductID;
                existingChapter.EpisodeChapterName = chapter.EpisodeChapterName;
                database.SaveChanges();

                // Redirect to Detail action with valid productId
                return RedirectToAction("Detail", "Detail", new { id = chapter.ProductID }); // Ensure id is being set correctly
            }

            return View(chapter);
        }

        // Trang Quản Lý Đánh Giá
        public ActionResult ManageReviews()
        {
            // Lấy danh sách đánh giá từ cơ sở dữ liệu
            var reviews = GetAllReviews();

            var model = reviews.Select(r => new CommentViewModel
            {
                ReviewID = r.ReviewID,
                ProductID = r.ProductID,
                Title = r.Title,
                Comment = r.Comment,
                ReviewDate = r.ReviewDate ?? DateTime.Now,
                UserID = r.UserID ?? 0,
                User = r.User,
                ParentReviewID = r.ParentReviewID,

                // Bổ sung thông tin lấy từ UserReview
                ProductName = r.Product?.Title ?? "Sản phẩm không tồn tại",
                UserName = r.User?.FullName ?? "Người dùng không tồn tại"
            }).ToList();

            return View(model);
        }

        private List<UserReview> GetAllReviews()
        {
            // Lấy danh sách đánh giá từ cơ sở dữ liệu
            var reviews = database.UserReviews.Include("Product").Include("User").ToList();
            return reviews;
        }


        // Thêm đánh giá
        public ActionResult AddReview()
        {
            return View();
        }

      
        // Xóa đánh giá
        [HttpGet]
        public ActionResult DeleteReview(int id)
        {
            var review = database.UserReviews.Find(id);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Đánh giá không tồn tại.";
                return RedirectToAction("ManageReviews");
            }

            database.UserReviews.Remove(review);
            database.SaveChanges();

            return RedirectToAction("ManageReviews");
        }

        // Sửa đánh giá
        public ActionResult EditReview(int id)
        {
            var review = database.UserReviews.Find(id);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Đánh giá không tồn tại.";
                return RedirectToAction("ManageReviews");
            }

            var model = new CommentViewModel
            {
                ProductID = review.ProductID,
                Title = review.Title,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate ?? DateTime.Now,
                UserID = review.UserID ?? 0,
                ParentReviewID = review.ParentReviewID
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult EditReview(CommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingReview = database.UserReviews.Find(model.ProductID);
                if (existingReview == null)
                {
                    TempData["ErrorMessage"] = "Đánh giá không tồn tại.";
                    return RedirectToAction("ManageReviews");
                }

                // Cập nhật các thuộc tính
                existingReview.ProductID = model.ProductID;
                existingReview.Title = model.Title;
                existingReview.Comment = model.Comment;
                existingReview.ReviewDate = model.ReviewDate;
                existingReview.ParentReviewID = model.ParentReviewID;

                database.SaveChanges();

                return RedirectToAction("ManageReviews");
            }

            return View(model);
        }


    }
}
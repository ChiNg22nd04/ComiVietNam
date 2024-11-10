using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class ProfileController : Controller
    {
        WebStory2Entities1 database = new WebStory2Entities1();
        // GET: Profile
        public ActionResult ProfileUser()
        {
            // Giả sử bạn đã lưu thông tin người dùng trong session khi đăng nhập
            var email = Session["Email"]?.ToString();

            // Kiểm tra xem username có phải null không
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login"); // Chuyển hướng đến trang đăng nhập nếu không có người dùng
            }

            var user = database.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Tạo ProfileViewModel từ thông tin người dùng
            var model = new User
            {
                UserID = user.UserID,
                FullName = user.FullName,
                UserName = user.UserName,
                Avatar = user.Avatar,
                Email = user.Email,
                RoleUser = user.RoleUser,
                Categories = CategoryService.GetAllCategories() // Lấy danh sách danh mục
            };

            return View(model);
        }


        public ActionResult ChangePassword(int userID)
        {
            // Lấy thông tin người dùng nếu cần, hoặc chỉ trả về view
            var user = database.Users.Find(userID);
            if (user == null)
            {
                return HttpNotFound(); // Nếu người dùng không tồn tại
            }

            var model = new ChangePasswordViewModel
            {
                UserID = userID // Gán UserID vào model để sử dụng trong View
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = database.Users.Find(model.UserID); // Lấy người dùng từ DbContext

                // Kiểm tra xem người dùng có tồn tại không
                if (user == null)
                {
                    ModelState.AddModelError("", "Người dùng không tồn tại.");
                    return View(model);
                }

                // Kiểm tra mật khẩu mới có trùng khớp với xác nhận không
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu mới và xác nhận không khớp.");
                    return View(model);
                }

                // Cập nhật mật khẩu mới (không băm)
                user.PasswordHash = model.NewPassword; // Cập nhật mật khẩu mới trực tiếp
                database.SaveChanges(); // Lưu thay đổi vào DbContext
                ViewBag.Message = "Mật khẩu đã được thay đổi thành công!";

                // Chuyển hướng sau khi thành công
                return RedirectToAction("ProfileUser", new { userID = model.UserID });
            }

            return View(model);
        }


        //Chưa sửa xong đổi mật khẩu

        public ActionResult EditProfile(int id)
        {
            var user = database.Users.Find(id);
            var categories = CategoryService.GetAllCategories();

            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("ProfileUser", "Profile");
            }

            ViewBag.Categories = categories;
            return View(user);
        }

        [HttpPost]
        public ActionResult EditProfile(User user, HttpPostedFileBase AvatarFile)
        {
            if (ModelState.IsValid)
            {
                var existingUser = database.Users.Find(user.UserID);

                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                    return RedirectToAction("ProfileUser", "Profile");
                }

                // Cập nhật các trường đã thay đổi
                if (user.FullName != existingUser.FullName)
                {
                    existingUser.FullName = user.FullName;
                }

                if (user.Email != existingUser.Email)
                {
                    existingUser.Email = user.Email;
                }

                // Kiểm tra và cập nhật avatar nếu có
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    var fileName = $"{user.UserID}_{Path.GetFileName(AvatarFile.FileName)}";
                    var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);

                    // Lưu file avatar
                    AvatarFile.SaveAs(path);
                    existingUser.Avatar = $"/Content/images/{fileName}";
                }
                else if (string.IsNullOrEmpty(existingUser.Avatar))
                {
                    // Nếu không có ảnh mới và avatar hiện tại trống, giữ lại avatar cũ
                    existingUser.Avatar = existingUser.Avatar ?? "/Content/images/profile1.png";
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                database.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction("ProfileUser", "Profile");
            }

            // Nếu model không hợp lệ, trả lại form với lỗi
            return View(user);
        }


    }
}
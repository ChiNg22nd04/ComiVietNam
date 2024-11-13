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
            var email = Session["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            var user = database.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new User
            {
                UserID = user.UserID,
                FullName = user.FullName,
                UserName = user.UserName,
                Avatar = user.Avatar,
                Email = user.Email,
                RoleUser = user.RoleUser,
                Categories = CategoryService.GetAllCategories()
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
        public ActionResult EditProfile(int? id)
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("ProfileUser");
            }

            var user = database.Users.Find(id.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("ProfileUser");
            }

            ViewBag.Categories = CategoryService.GetAllCategories();
            return View(user);
        }

        [HttpPost]
        public ActionResult EditProfile(User user, HttpPostedFileBase AvatarFile)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng trong cơ sở dữ liệu theo UserID
                var existingUser = database.Users.Find(user.UserID);
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                    return RedirectToAction("ProfileUser");
                }

                // Cập nhật các trường thông tin cá nhân
                existingUser.FullName = user.FullName ?? existingUser.FullName;
                existingUser.Email = user.Email ?? existingUser.Email;

                // Kiểm tra xem người dùng có upload ảnh avatar mới không
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    var extension = Path.GetExtension(AvatarFile.FileName);
                    var fileName = $"{user.UserID}_{Path.GetFileNameWithoutExtension(AvatarFile.FileName)}{extension}";
                    var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    AvatarFile.SaveAs(path);
                    existingUser.Avatar = $"/Content/images/{fileName}"; // Cập nhật Avatar mới vào cơ sở dữ liệu
                }

                // Lưu lại thông tin thay đổi vào cơ sở dữ liệu
                database.SaveChanges();  // Đây là bước quan trọng để lưu thông tin vào cơ sở dữ liệu

                // Sau khi lưu thành công vào cơ sở dữ liệu, cập nhật lại session
                Session["FullName"] = existingUser.FullName;
                Session["Email"] = existingUser.Email;

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction("ProfileUser"); // Chuyển hướng về trang hồ sơ người dùng
            }

            return View(user);
        }


    }
}
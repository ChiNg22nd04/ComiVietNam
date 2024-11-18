using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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
                    TempData["ErrorMessage"] = ("Người dùng không tồn tại.");
                    return View();
                }

                // Kiểm tra mật cũ mới có trùng khớp trong data không
                if (model.OldPassword != user.PasswordHash)
                {
                    TempData["ErrorMessage"] = ("Mật khẩu cũ không trùng khớp.");
                    return View();
                }

                // Kiểm tra mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số
                if (!IsValidPassword(model.NewPassword))
                {
                    TempData["ErrorMessage"] = "Mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số.";
                    return View();
                }

                // Kiểm tra mật khẩu mới có trùng khớp với xác nhận không
                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = ("Mật khẩu mới và mật khẩu xác nhận không khớp.");
                    return View();
                }

                // Cập nhật mật khẩu mới (không băm)
                user.PasswordHash = model.NewPassword; // Cập nhật mật khẩu mới trực tiếp
                database.SaveChanges(); // Lưu thay đổi vào DbContext
                TempData["SuccessMessage"] = "Mật khẩu đã được cập nhật thành công!";

                // Chuyển hướng sau khi thành công
                return RedirectToAction("ProfileUser", new { userID = model.UserID });
            }

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

            Debug.WriteLine($"UserID: {user.UserID}",user);
            Debug.WriteLine($"Fullname: {user.FullName}");


            ViewBag.Categories = CategoryService.GetAllCategories();
            return View(user);
        }

        [HttpPost]
        public ActionResult EditProfile(User user, HttpPostedFileBase AvatarFile)
        {

            Debug.WriteLine("ModelStat", ModelState.IsValid);
            if (ModelState.IsValid)
            {
                // Log user info
                Debug.WriteLine($"UserID: {user.UserID}");
                Debug.WriteLine($"FullName: {user.FullName}");
                Debug.WriteLine($"Email: {user.Email}");

                // Check for avatar file
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    Debug.WriteLine($"Avatar File Name: {AvatarFile.FileName}");
                    Debug.WriteLine($"Avatar File Size: {AvatarFile.ContentLength}");

                    string fileName = Path.GetFileName(AvatarFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    AvatarFile.SaveAs(path);

                    user.Avatar = "/Content/images/" + fileName;
                    Debug.WriteLine($"Updated Avatar: {user.Avatar}");
                }

                // Update user in database
                var existingUser = database.Users.FirstOrDefault(u => u.UserID == user.UserID);
                if (existingUser != null)
                {
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Avatar = user.Avatar ?? existingUser.Avatar;

                    database.SaveChanges();

                    Debug.WriteLine($"Updated User: {existingUser.UserID}, {existingUser.FullName}, {existingUser.Email}, {existingUser.Avatar}");

                    return RedirectToAction("ProfileUser", "Profile", new { userID = user.UserID });
                }
                else
                {
                    ViewBag.Error = "User not found!";
                    Debug.WriteLine("Error: User not found.");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
            }

            ViewBag.UserID = user.UserID;
            Debug.WriteLine($"Error occurred. UserID: {user.UserID}");

            return View(user);
        }


        public ActionResult UserComments(int userID)
        {
            var user = database.Users.Find(userID);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("ProfileUser");
            }

            var comments = database.UserReviews
                .Where(r => r.UserID == userID)
                .ToList();  

            var viewModel = new UserCommentsViewModel
            {
                User = user,
                Comments = comments
            };
            return View("UserReview", viewModel);
        }


    }
}
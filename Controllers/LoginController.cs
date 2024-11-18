using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class LoginController : Controller
    {
        WebStory2Entities1 database = new WebStory2Entities1();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        //Hàm get form login lên
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]

        public ActionResult Login(User user)
        {
            // Xác thực thông tin đăng nhập
            var check = database.Users.FirstOrDefault(s => s.Email.Equals(user.Email)
                    && s.PasswordHash.Equals(user.PasswordHash));
            if (check != null)
            {
                Session["CurrentUserId"] = user.UserID; // Lưu ID người dùng vào Session
                Session["UserID"] = check.UserID;
                Session["FullName"] = check.FullName;
                Session["RoleUser"] = check.RoleUser;  // Lưu role từ database vào session
                Session["Username"] = check.UserName;
                Session["Avatar"] = check.Avatar;  // Lưu avatar từ database vào session
                Session["Email"] = check.Email;  // Lưu avatar từ database vào session


                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.error = "Email hoặc mật khẩu không đúng";
                return View("Login", user);
            }
        }

        [HttpGet]
        //Hàm get form register lên
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email đã tồn tại hay chưa
                var check = database.Users.Where(s => s.Email == user.Email).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.error = "Email đã tồn tại";
                    return View();
                }

                // Kiểm tra mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số
                if (!IsValidPassword(user.PasswordHash))
                {
                    ViewBag.error = "Mật khẩu phải dài ít nhất 8 ký tự và bao gồm cả chữ lẫn số.";
                    return View();
                }

                // Kiểm tra mật khẩu nhập lại
                if (user.PasswordHash != user.ConfirmPassword)
                {
                    ViewBag.error = "Mật khẩu xác nhận không khớp.";
                    return View();
                }

                // Đặt role mặc định là "KhachHang" khi đăng ký
                user.RoleUser = "Khách hàng";
                user.RegisteredDate = DateTime.Now;
                database.Configuration.ValidateOnSaveEnabled = false;
                database.Users.Add(user);
                database.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.error = "Vui lòng điền đầy đủ thông tin và thử lại.";
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

        public ActionResult Logout(User user)
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(string email)
        {
            using (var db = new WebStory2Entities1())
            {
                var user = db.Users.SingleOrDefault(u => u.Email == email);
                if (user != null)
                {
                    TempData["Email"] = email; // Lưu email tạm thời
                    TempData.Keep();
                    return RedirectToAction("ResetPassword");
                }
                ViewBag.error = "Email không tồn tại!";
            }
            return View();
        }


        public ActionResult ResetPassword()
        {
            if (TempData["Email"] == null)
                return RedirectToAction("ForgetPassword");

            TempData.Keep(); // Giữ lại TempData để dùng trong POST
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            // Kiểm tra độ dài mật khẩu
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.error = "Mật khẩu phải có ít nhất 6 ký tự!";
                TempData.Keep(); // Giữ lại TempData cho lần request tiếp theo
                return View();
            }

            // Kiểm tra mật khẩu xác nhận
            if (newPassword != confirmPassword)
            {
                ViewBag.error = "Mật khẩu không khớp!";
                TempData.Keep();
                return View();
            }

            using (var db = new WebStory2Entities1())
            {
                string email = TempData["Email"].ToString(); // Lấy email từ TempData
                var user = db.Users.SingleOrDefault(u => u.Email == email);

                if (user != null)
                {
                    user.PasswordHash = newPassword; // Lưu trực tiếp mật khẩu mới
                    db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    return RedirectToAction("Login", "Login"); // Chuyển hướng về trang đăng nhập
                }

                ViewBag.error = "Có lỗi xảy ra! Vui lòng thử lại.";
            }

            return View();
        }
    }
}
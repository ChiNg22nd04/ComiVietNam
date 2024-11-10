using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAPM2.Models;

namespace DAPM2.Controllers
{
    public class CommentController : Controller
    {
        WebStory2Entities1 db = new WebStory2Entities1();
        // GET: Comment
        public ActionResult CommentStory()
        {
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult CommentStory(CommentViewModel commentViewModel)
        {

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra và thử lại.";
                return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
            }

            try
            {
                using (var context = new WebStory2Entities1())
                {
                    // Kiểm tra xem UserID có tồn tại trong session
                    if (Session["UserID"] == null)
                    {
                        TempData["ErrorMessage"] = "Người dùng chưa đăng nhập.";
                        return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
                    }

                    commentViewModel.UserID = (int)Session["UserID"]; // Lấy UserID từ session

                    // Kiểm tra xem UserID có tồn tại trong bảng Users
                    var userExists = context.Users.Any(u => u.UserID == commentViewModel.UserID);
                    if (!userExists)
                    {
                        TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                        return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
                    }

                    // Tạo một đối tượng UserReview mới
                    var userReview = new UserReview
                    {
                        UserID = commentViewModel.UserID,
                        ProductID = commentViewModel.ProductID,
                        Comment = commentViewModel.Comment,
                        ReviewDate = DateTime.Now
                    };

                    context.UserReviews.Add(userReview);
                    context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                }

                TempData["SuccessMessage"] = "Bình luận đã được gửi thành công!";
                return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.InnerException?.Message;
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu bình luận: " + innerException;
                return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID });
            }
        }

        public ActionResult ReplyComment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReplyComment(CommentViewModel commentViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var context = new WebStory2Entities1()) // Replace with your actual context
                {
                    var reply = new UserReview
                    {
                        UserID = commentViewModel.UserID,             // Use UserID from ViewModel
                        ProductID = commentViewModel.ProductID,       // Use ProductID from ViewModel
                        Comment = commentViewModel.Comment,            // Use Comment from ViewModel
                        ParentReviewID = commentViewModel.ParentReviewID, // Use ParentReviewID from ViewModel
                        ReviewDate = DateTime.Now
                    };

                    context.UserReviews.Add(reply);
                    context.SaveChanges();
                }
                return RedirectToAction("Detail", "Detail", new { id = commentViewModel.ProductID }); // Redirect to the appropriate action after saving
            }

            // If model state is invalid, return to the same view with error messages
            return View(commentViewModel);
        }

    }
}
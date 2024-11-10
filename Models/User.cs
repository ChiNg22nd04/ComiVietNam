﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAPM2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.ReadingHistories = new HashSet<ReadingHistory>();
            this.UserReviews = new HashSet<UserReview>();
            this.Wishlists = new HashSet<Wishlist>();
        }

        public int UserID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }
        public string Avatar { get; set; }


        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên.", MinimumLength = 6)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("PasswordHash", ErrorMessage = "Mật khẩu và nhập lại mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
        public string RoleUser { get; set; }
        public Nullable<System.DateTime> RegisteredDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserReview> UserReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Wishlist> Wishlists { get; set; }
        public List<Category> Categories { get; set; } // Thêm thu?c tính Categories

    }
    public class ManageUsersViewModel
    {
        // Thu?c tính ?? l?u danh sách ng??i dùng
        public List<User> Users { get; set; }
    }
    public class UserViewModel
    {
        public User user { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public List<Category> Categories { get; set; } // Thêm thu?c tính Categories
        public List<UpdateChapter> Chapters { get; set; } // Thêm thu?c tính này
    }

    public class ChangePasswordViewModel
    {
        public int UserID { get; set; } // ID ng??i dùng hi?n t?i

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
        public List<Category> categories { get; set; }
    }
}

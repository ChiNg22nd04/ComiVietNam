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
    using System.Linq;
    using System.Data.Entity;


    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.ReadingHistories = new HashSet<ReadingHistory>();
            this.UpdateChapters = new HashSet<UpdateChapter>();
            this.UserReviews = new HashSet<UserReview>();
            this.Wishlists = new HashSet<Wishlist>();
        }

        public int ProductID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public Nullable<int> PublicationYear { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public string Content { get; set; }
        public string ImageProcductURL { get; set; }
        public Nullable<int> ViewCount { get; set; }

        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UpdateChapter> UpdateChapters { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserReview> UserReviews { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Wishlist> Wishlists { get; set; }

    }
    public static class ProductService
    {
        public static List<Product> GetStoriesByCategory(int categoryID)
        {
            using (var db = new WebStory2Entities1())
            {
                return db.Products
                         .Where(p => p.CategoryID == categoryID)
                         .ToList();
            }
        }

        public static Product GetProductById(int productID)
        {
            using (var db = new WebStory2Entities1())
            {
                return db.Products
                         .Include(p => p.Category)
                         .Include(p => p.UserReviews) // Include related UserReviews
                         .FirstOrDefault(c => c.ProductID == productID);
            }
        }

        public static void IncreaseViewCount(int productID)
        {
            using (var db = new WebStory2Entities1())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductID == productID);
                if (product != null)
                {
                    product.ViewCount = (product.ViewCount ?? 0) + 1;
                    db.SaveChanges();
                }
            }
        }

        public static List<Product> SearchBooksByTitle(string searchTerm)
        {
            using (var db = new WebStory2Entities1())
            {
                // Tìm kiếm sách dựa trên tiêu đề
                return db.Products
                         .Where(p => p.Title.Contains(searchTerm)) // Tìm kiếm theo từ khóa trong tiêu đề
                         .ToList();
            }
        }
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public Category Category { get; set; }
        public List<Category> Categories { get; set; } // List of categories
        public List<UpdateChapter> Chapters { get; set; } // List of chapters
        public List<UserReview> UserReviews { get; set; }
        public bool IsInWishlist { get; set; } // Thêm thuộc tính này

        public ProductDetailViewModel()
        {
            UserReviews = new List<UserReview>();
            Chapters = new List<UpdateChapter>(); // Initialize chapters
            Categories = new List<Category>(); // Initialize categories
        }

        public string GetDateAddedText(DateTime? dateAdded)
        {
            if (!dateAdded.HasValue)
                return "Chưa có thông tin ngày";

            var timeDifference = DateTime.Now - dateAdded.Value;

            if (timeDifference.TotalSeconds < 60)
                return $"{(int)timeDifference.TotalSeconds} giây trước";
            else if (timeDifference.TotalMinutes < 60)
                return $"{(int)timeDifference.TotalMinutes} phút trước";
            else if (timeDifference.TotalHours < 24)
                return $"{(int)timeDifference.TotalHours} giờ trước";
            else if (timeDifference.TotalDays < 30)
                return $"{(int)timeDifference.TotalDays} ngày trước";
            else if (timeDifference.TotalDays < 365)
                return $"{(int)(timeDifference.TotalDays / 30)} tháng trước";
            else
                return $"{(int)(timeDifference.TotalDays / 365)} năm trước";
        }
    }


    public class StoryViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; } // List of categories
        public Product Product { get; set; }  // Current product
        public Category Category { get; set; }  // Current category
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public StoryViewModel()
        {
            Products = new List<Product>();
            Categories = new List<Category>(); // Initialize categories
        }
    }
}

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

    public partial class Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Category()
        {
            this.Products = new HashSet<Product>();
        }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
    }
    public class CategoryViewModel
    {
        // Thu?c tính ?? l?u danh sách ng??i dùng
        public List<Category> categories { get; set; }
        public List<Product> products { get; set; } // Thêm thu?c tính products
        public List<Product> recentlyAddedStories { get; set; }
        public List<Product> mostViewedStories { get; set; }
        public List<Product> recentlyUpdatedStories { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string GetDateAddedText(DateTime? dateAdded)
        {
            if (!dateAdded.HasValue)
                return "Chưa có thông tin ngày";

            var daysDifference = (DateTime.Now - dateAdded.Value).Days;

            if (daysDifference < 1)
                return "Hôm nay";
            else if (daysDifference == 1)
                return "Hôm qua";
            else
                return $"{daysDifference} ngày trước";
        }
    }
    // Ph??ng th?c này l?y category d?a trên ID
    public class CategoryService
    {
        // Ph??ng th?c này l?y category d?a trên ID
        public static Category GetCategoryById(int categoryID)
        {
            using (var db = new WebStory2Entities1())
            {
                return db.Categories.FirstOrDefault(c => c.CategoryID == categoryID);
            }
        }
        public static List<Category> GetAllCategories()
        {
            using (var db = new WebStory2Entities1())
            {
                return db.Categories.ToList();
            }
        }
    }
}

//------------------------------------------------------------------------------
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

    public partial class Wishlist
    {
        public int WishlistID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> ProductID { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }

        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
    public class WishlistViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; } // ??m b?o Category kh�ng null khi s? d?ng
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}

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
    using System.Linq;

    public partial class ChapterImage
    {
        public int ImageID { get; set; }
        public Nullable<int> ChapterID { get; set; }
        public string ImageURL { get; set; }
        public Nullable<int> ImageOrder { get; set; }
        public Nullable<System.DateTime> UploadedDate { get; set; }

        public virtual UpdateChapter UpdateChapter { get; set; }
    }
    public static class ChapterImageService
    {
        public static List<ChapterImage> GetImagesByChapterId(int chapterId)
        {
            using (var context = new WebStory2Entities1())
            {
                var images = context.ChapterImages
                    .Where(ci => ci.ChapterID == chapterId)
                    .ToList();

                // Ki?m tra t?ng URL c?a h�nh ?nh
                foreach (var img in images)
                {
                    Console.WriteLine(img.ImageURL);
                }

                return images;
            }
        }
    }


    public class ImageDetailViewModel
    {
        public Product Product { get; set; } = new Product(); // Kh?i t?o ??i t??ng m?c ??nh
        public Category Category { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>(); // Kh?i t?o danh s�ch r?ng
        public List<UpdateChapter> Chapters { get; set; } = new List<UpdateChapter>(); // Kh?i t?o danh s�ch r?ng
        public UpdateChapter PreviousChapter { get; set; }
        public UpdateChapter NextChapter { get; set; }
        public UpdateChapter CurrentChapter { get; set; }
    }
}

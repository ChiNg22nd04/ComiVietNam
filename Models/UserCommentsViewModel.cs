using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DAPM2.Models
{
    public class UserCommentsViewModel
    {
        public DAPM2.Models.User User { get; set; }  
        public List<DAPM2.Models.UserReview> Comments { get; set; }  
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Erth.Shared
{
    public class UserInfo
    {
        [Required(ErrorMessage="تایید یا عدم تایید هویت را مشخص کنید")]
        public bool IsAuthenticated { get; set; }
        
        [Required(ErrorMessage="نام کاربری را بنویسید")]
        public string UserName { get; set; }
        
        public Dictionary<string, string> ExposedClaims { get; set; }
        
        [Required(ErrorMessage="مدیر یا کاربر عادی بودن باید مشخص باشد")]
        public bool IsAdmin { get; set; }
    }
}

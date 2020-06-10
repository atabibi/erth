using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Erth.Shared
{
    public class LoginParameters
    {
        [Required(ErrorMessage="نام کاربری را وارد کنید")]
        public string UserName { get; set; }

        [Required(ErrorMessage="رمز عبور را وارد کنید")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

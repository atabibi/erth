using System;
using System.ComponentModel.DataAnnotations;

namespace Erth.Shared.Models
{
    public class ChangePasswordVM
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        
        [Required(ErrorMessage="رمز فعلی را وارد کنید")]
        public string OldPassword { get; set; }
        
        [Required(ErrorMessage="رمز جدید را وارد کنید")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{6,}$",ErrorMessage="رمز جدید باید حداق ۶ حرف شامل عدد حروف خاص و حروف انگلیسی باشد")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage="تکرار رمز جدید را وارد کنید")]
        [DataType(DataType.Password)]
        [Compare("NewPassword",ErrorMessage="تکرار رمز جدید با رمز جدید یکسان نیست")]
        public string RepeatePassword { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Erth.Shared.Models
{
    ///
    // واگذاری یک لیبل سی دی به یک مشتری
    ///
    public class NewCdLabelVM
    {
        [Required(ErrorMessage="نام و نام خانوادگی مشتری را وارد نکرده‌اید.")]
        [StringLength(35,ErrorMessage="نام حداکثر می تواند ۳۵ حرف باشد")]
        public string CustomerFullName { get; set; }

        [Required(ErrorMessage="استان محل سکونت مشتری را مشخص نکرده‌اید.")]
        [StringLength(25,ErrorMessage="نام استان حداکثر ۲۵ حرف می‌تواند باشد")]
        public string CustomerUnited { get; set; }
        
        [Required(ErrorMessage="نسخه نرم‌افزار را انتخاب نکرده‌اید")]
        public CdTypeErth TypeErth { get; set; }
    }
}
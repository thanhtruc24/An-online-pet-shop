using System.ComponentModel.DataAnnotations;

namespace CHTC_1.Areas.Admin.Models
{
    public class AdminLoginViewModel
    {
        [Key]
        [MaxLength(100)]
        [Required(ErrorMessage = ("Vui lòng nhập Email"))]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]
        public string Email { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(5, ErrorMessage = "Sai tài khoản hoặc mật khẩu")]
        public string Password
        {
            get; set;

        }
    }
}

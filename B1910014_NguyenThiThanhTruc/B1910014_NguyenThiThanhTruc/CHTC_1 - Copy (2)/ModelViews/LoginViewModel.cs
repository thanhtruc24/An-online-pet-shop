using System.ComponentModel.DataAnnotations;

namespace CHTC_1.ModelViews
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(100)]
        [Required(ErrorMessage = ("Vui lòng nhập Email"))]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]
        public string Email { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(5, ErrorMessage = "Vui lòng nhập mật khẩu hợp lệ")]
        public string Password
        {
            get; set;

        }
    }
}

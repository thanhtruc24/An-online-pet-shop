using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CHTC_1.Areas.Admin.Models
{
    public class CreateAccount
    {
        public int RoleId { get; set; }

        [Display(Name = "Họ và Tên")]
        [Required(ErrorMessage = "Vui lòng nhập Họ Tên")]
        public string FullName { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [MaxLength(150)]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [DataType(DataType.EmailAddress)]
        //[Remote(action: "ValidateEmail", controller: "Accounts")]
        public string Email { get; set; }



        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(5, ErrorMessage = "Bạn cần đặt mật khẩu tối thiểu 5 ký tự")]
        public string Password { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? Avatar { get; set; }

    }
}

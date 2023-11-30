using CHTC_1.Models;
using System.ComponentModel.DataAnnotations;

namespace CHTC_1.ModelViews
{
    public class MuaHangViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên")]
        public string FullName { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ nhận hàng")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phương thức thanh toán")]
        public int PaymentMethod { get; set; }
    }
}

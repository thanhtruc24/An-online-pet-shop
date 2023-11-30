using CHTC_1.Models;

namespace CHTC_1.ModelViews
{
    public class SanPhamViewModel
    {
        public SanPham? SanPham { get; set; }
        public List<DanhGium>? DanhGia { get; set; }
        public string NoiDung { get; set; }
        public int? Diem { get; set; }
        public int SpId { get; set; }
        public string? Avatar { get; set; }
    }
}

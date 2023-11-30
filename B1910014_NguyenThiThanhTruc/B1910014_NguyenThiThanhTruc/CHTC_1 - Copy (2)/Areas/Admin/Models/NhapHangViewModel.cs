using CHTC_1.Models;
using PagedList.Core;

namespace CHTC_1.Areas.Admin.Models
{
    public class NhapHangViewModel
    {
        public PagedList<SanPham> sanPhams { get; set; }

        public SanPham sanPham { get; set; }

        public List<ChitietPhieuNhap> chitietPhieuNhap { get; set; }
    }
}

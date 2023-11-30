using CHTC_1.Models;

namespace CHTC_1.ModelViews
{
    public class CartItem
    {
        public SanPham SanPham { get; set; }
        public int Soluong { get; set; }
        public decimal Tongtien => Soluong * SanPham.SpGia;
        //public decimal Tongtien()
        //{
        //    return Soluong * SanPham.SpGia;
        //}
    }
}

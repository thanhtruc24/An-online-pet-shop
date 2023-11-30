using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class NguoiDung
{
    public int NdId { get; set; }

    public string NdHoten { get; set; } = null!;

    public string? NdHinhanh { get; set; }

    public string? NdSdt { get; set; }

    public string? NdDiachi { get; set; }

    public int? NdGioitinh { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; } = new List<DanhGium>();

    public virtual ICollection<HoaDon> HoaDons { get; } = new List<HoaDon>();

    public virtual ICollection<PhieuNhapHang> PhieuNhapHangs { get; } = new List<PhieuNhapHang>();

    public virtual ICollection<TaiKhoan> TaiKhoans { get; } = new List<TaiKhoan>();
}

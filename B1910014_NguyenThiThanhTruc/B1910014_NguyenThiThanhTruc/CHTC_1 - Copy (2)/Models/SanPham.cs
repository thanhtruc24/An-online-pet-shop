using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class SanPham
{
    public int SpId { get; set; }

    public int? NccId { get; set; }

    public int? LId { get; set; }

    public string SpTensp { get; set; } = null!;

    public decimal SpGia { get; set; }

    public string? SpMota { get; set; }

    public string? SpHinhanh { get; set; }

    public int? SpSoluongton { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; } = new List<ChiTietHoaDon>();

    public virtual ICollection<ChitietPhieuNhap> ChitietPhieuNhaps { get; } = new List<ChitietPhieuNhap>();

    public virtual ICollection<DanhGium> DanhGia { get; } = new List<DanhGium>();

    public virtual Loai? LIdNavigation { get; set; }

    public virtual NhaCungCap? Ncc { get; set; }
}

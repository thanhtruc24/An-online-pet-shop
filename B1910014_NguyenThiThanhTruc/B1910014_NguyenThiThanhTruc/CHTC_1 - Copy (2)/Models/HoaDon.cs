using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class HoaDon
{
    public int HdId { get; set; }

    public int? PtttId { get; set; }

    public int? NdId { get; set; }

    public int? TthdId { get; set; }

    public decimal HdTongtien { get; set; }

    public DateTime HdNgay { get; set; }

    public int HdThanhtoan { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; } = new List<ChiTietHoaDon>();

    public virtual NguoiDung? Nd { get; set; }

    public virtual PhuongThucThanhToan? Pttt { get; set; }

    public virtual TrangThaiHoaDon? Tthd { get; set; }
}

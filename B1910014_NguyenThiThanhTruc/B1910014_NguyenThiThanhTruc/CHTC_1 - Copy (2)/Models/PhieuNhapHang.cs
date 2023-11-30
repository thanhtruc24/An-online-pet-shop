using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class PhieuNhapHang
{
    public int PnhId { get; set; }

    public DateTime PnhNgaynhap { get; set; }

    public decimal PndDongia { get; set; }

    public int? NdId { get; set; }

    public virtual ICollection<ChitietPhieuNhap> ChitietPhieuNhaps { get; } = new List<ChitietPhieuNhap>();

    public virtual NguoiDung? Nd { get; set; }
}

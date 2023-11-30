using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class ChitietPhieuNhap
{
    public int PnhId { get; set; }

    public int SpId { get; set; }

    public int? CtpnSoluong { get; set; }

    public decimal CtpnGiagoc { get; set; }

    public virtual PhieuNhapHang Pnh { get; set; } = null!;

    public virtual SanPham Sp { get; set; } = null!;
}

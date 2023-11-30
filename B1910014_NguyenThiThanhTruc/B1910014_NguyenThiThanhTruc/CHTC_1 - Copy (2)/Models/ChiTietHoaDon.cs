using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class ChiTietHoaDon
{
    public int SpId { get; set; }

    public int HdId { get; set; }

    public int CthdSoluong { get; set; }

    public virtual HoaDon Hd { get; set; } = null!;

    public virtual SanPham Sp { get; set; } = null!;
}

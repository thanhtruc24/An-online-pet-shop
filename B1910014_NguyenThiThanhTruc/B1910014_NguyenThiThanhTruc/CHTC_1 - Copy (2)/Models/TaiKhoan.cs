using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class TaiKhoan
{
    public string TkEmail { get; set; } = null!;

    public int? VtId { get; set; }

    public int? NdId { get; set; }

    public string? TkMatkhau { get; set; }

    public virtual NguoiDung? Nd { get; set; }

    public virtual VaiTro? Vt { get; set; }
}

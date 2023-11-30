using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class VaiTro
{
    public int VtId { get; set; }

    public string VtTenvaitro { get; set; } = null!;

    public virtual ICollection<TaiKhoan> TaiKhoans { get; } = new List<TaiKhoan>();

    public virtual ICollection<QuyenSuDung> Qsds { get; } = new List<QuyenSuDung>();
}

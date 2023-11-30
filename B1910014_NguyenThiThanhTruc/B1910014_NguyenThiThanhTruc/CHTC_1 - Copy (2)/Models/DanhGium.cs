using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class DanhGium
{
    public int NdId { get; set; }

    public int SpId { get; set; }

    public int DgId { get; set; }

    public string? DgNoidung { get; set; }

    public string? DgHinhanh { get; set; }

    public DateTime DgNgay { get; set; }

    public int? DgSao { get; set; }

    public int? DgThich { get; set; }

    public int? DgKhongthich { get; set; }

    public virtual NguoiDung Nd { get; set; } = null!;

    public virtual SanPham Sp { get; set; } = null!;
}

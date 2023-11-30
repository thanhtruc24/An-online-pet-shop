using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class PhuongThucThanhToan
{
    public int PtttId { get; set; }

    public string PtttTen { get; set; } = null!;

    public virtual ICollection<HoaDon> HoaDons { get; } = new List<HoaDon>();
}

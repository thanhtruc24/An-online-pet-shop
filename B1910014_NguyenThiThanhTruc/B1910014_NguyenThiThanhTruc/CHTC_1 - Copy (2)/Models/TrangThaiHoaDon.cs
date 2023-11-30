using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class TrangThaiHoaDon
{
    public int TthdId { get; set; }

    public string TthdTrangthai { get; set; } = null!;

    public string TthdMota { get; set; } = null!;

    public virtual ICollection<HoaDon> HoaDons { get; } = new List<HoaDon>();
}

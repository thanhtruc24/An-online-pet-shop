using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class LoaiQuyen
{
    public int LqId { get; set; }

    public string? LqTenloaiquyen { get; set; }

    public virtual ICollection<QuyenSuDung> QuyenSuDungs { get; } = new List<QuyenSuDung>();
}

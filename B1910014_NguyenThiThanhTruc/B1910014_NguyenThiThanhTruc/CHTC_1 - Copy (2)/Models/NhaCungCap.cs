using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class NhaCungCap
{
    public int NccId { get; set; }

    public string NccTen { get; set; } = null!;

    public virtual ICollection<SanPham> SanPhams { get; } = new List<SanPham>();
}

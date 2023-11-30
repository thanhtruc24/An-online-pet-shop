using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class Loai
{
    public int LId { get; set; }

    public string LTenloai { get; set; } = null!;

    public virtual ICollection<SanPham> SanPhams { get; } = new List<SanPham>();
}

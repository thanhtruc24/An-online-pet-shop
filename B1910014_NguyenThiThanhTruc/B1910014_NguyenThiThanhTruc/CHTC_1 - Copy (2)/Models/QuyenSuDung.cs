using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class QuyenSuDung
{
    public int QsdId { get; set; }

    public int? LqId { get; set; }

    public string QsdTen { get; set; } = null!;

    public virtual LoaiQuyen? Lq { get; set; }

    public virtual ICollection<VaiTro> Vts { get; } = new List<VaiTro>();
}

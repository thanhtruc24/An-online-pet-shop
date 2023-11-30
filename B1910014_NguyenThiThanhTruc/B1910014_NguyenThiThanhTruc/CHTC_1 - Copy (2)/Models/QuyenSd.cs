using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class QuyenSd
{
    public int QsdId { get; set; }

    public string QsdTenquyen { get; set; } = null!;

    public virtual ICollection<NguoiDung> NguoiDungs { get; } = new List<NguoiDung>();
}

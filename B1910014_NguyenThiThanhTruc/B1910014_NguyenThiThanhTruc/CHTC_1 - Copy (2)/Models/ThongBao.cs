using System;
using System.Collections.Generic;

namespace CHTC_1.Models;

public partial class ThongBao
{
    public int TbId { get; set; }

    public string? TbTieude { get; set; }

    public string? TbNoidung { get; set; }

    public DateTime? TbThoigian { get; set; }

    public int? TbTrangthai { get; set; }

    public int? TbNguoigui { get; set; }

    public int? TbNguoinhan { get; set; }

    public string? TbAvt { get; set; }
}

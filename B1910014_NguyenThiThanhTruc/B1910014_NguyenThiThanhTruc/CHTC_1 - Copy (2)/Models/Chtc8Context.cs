using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CHTC_1.Models;

public partial class Chtc8Context : DbContext
{
    public Chtc8Context()
    {
    }

    public Chtc8Context(DbContextOptions<Chtc8Context> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChitietPhieuNhap> ChitietPhieuNhaps { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<Loai> Loais { get; set; }

    public virtual DbSet<LoaiQuyen> LoaiQuyens { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<PhieuNhapHang> PhieuNhapHangs { get; set; }

    public virtual DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

    public virtual DbSet<QuyenSuDung> QuyenSuDungs { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TrangThaiHoaDon> TrangThaiHoaDons { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-763PS1F;Database=CHTC8;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => new { e.SpId, e.HdId });

            entity.ToTable("CHI_TIET_HOA_DON");

            entity.HasIndex(e => e.HdId, "CO_FK");

            entity.HasIndex(e => e.SpId, "RELATIONSHIP_2_FK");

            entity.Property(e => e.SpId).HasColumnName("SP_ID");
            entity.Property(e => e.HdId).HasColumnName("HD_ID");
            entity.Property(e => e.CthdSoluong).HasColumnName("CTHD_SOLUONG");

            entity.HasOne(d => d.Hd).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.HdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHI_TIET_CO_HOA_DON");

            entity.HasOne(d => d.Sp).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.SpId)
                .HasConstraintName("FK_CHI_TIET_RELATIONS_SAN_PHAM");
        });

        modelBuilder.Entity<ChitietPhieuNhap>(entity =>
        {
            entity.HasKey(e => new { e.PnhId, e.SpId });

            entity.ToTable("CHITIET_PHIEU_NHAP");

            entity.HasIndex(e => e.PnhId, "RELATIONSHIP_19_FK");

            entity.HasIndex(e => e.SpId, "RELATIONSHIP_20_FK");

            entity.Property(e => e.PnhId).HasColumnName("PNH_ID");
            entity.Property(e => e.SpId).HasColumnName("SP_ID");
            entity.Property(e => e.CtpnGiagoc)
                .HasColumnType("money")
                .HasColumnName("CTPN_GIAGOC");
            entity.Property(e => e.CtpnSoluong).HasColumnName("CTPN_SOLUONG");

            entity.HasOne(d => d.Pnh).WithMany(p => p.ChitietPhieuNhaps)
                .HasForeignKey(d => d.PnhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHITIET__RELATIONS_PHIEU_NH");

            entity.HasOne(d => d.Sp).WithMany(p => p.ChitietPhieuNhaps)
                .HasForeignKey(d => d.SpId)
                .HasConstraintName("FK_CHITIET__RELATIONS_SAN_PHAM");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => new { e.NdId, e.SpId, e.DgId }).IsClustered(false);

            entity.ToTable("DANH_GIA");

            entity.HasIndex(e => e.NdId, "RELATIONSHIP_10_FK");

            entity.HasIndex(e => e.SpId, "RELATIONSHIP_11_FK");

            entity.Property(e => e.NdId).HasColumnName("ND_ID");
            entity.Property(e => e.SpId).HasColumnName("SP_ID");
            entity.Property(e => e.DgId)
                .ValueGeneratedOnAdd()
                .HasColumnName("DG_ID");
            entity.Property(e => e.DgHinhanh)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DG_HINHANH");
            entity.Property(e => e.DgKhongthich).HasColumnName("DG_KHONGTHICH");
            entity.Property(e => e.DgNgay)
                .HasColumnType("datetime")
                .HasColumnName("DG_NGAY");
            entity.Property(e => e.DgNoidung).HasColumnName("DG_NOIDUNG");
            entity.Property(e => e.DgSao).HasColumnName("DG_SAO");
            entity.Property(e => e.DgThich).HasColumnName("DG_THICH");

            entity.HasOne(d => d.Nd).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.NdId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DANH_GIA_RELATIONS_NGUOI_DU");

            entity.HasOne(d => d.Sp).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.SpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DANH_GIA_RELATIONS_SAN_PHAM");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.HdId).IsClustered(false);

            entity.ToTable("HOA_DON");

            entity.HasIndex(e => e.NdId, "QUAN_LY_FK");

            entity.HasIndex(e => e.TthdId, "RELATIONSHIP_12_FK");

            entity.HasIndex(e => e.PtttId, "RELATIONSHIP_5_FK");

            entity.Property(e => e.HdId).HasColumnName("HD_ID");
            entity.Property(e => e.HdNgay)
                .HasColumnType("datetime")
                .HasColumnName("HD_NGAY");
            entity.Property(e => e.HdThanhtoan).HasColumnName("HD_THANHTOAN");
            entity.Property(e => e.HdTongtien)
                .HasColumnType("money")
                .HasColumnName("HD_TONGTIEN");
            entity.Property(e => e.NdId).HasColumnName("ND_ID");
            entity.Property(e => e.PtttId).HasColumnName("PTTT_ID");
            entity.Property(e => e.TthdId).HasColumnName("TTHD_ID");

            entity.HasOne(d => d.Nd).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.NdId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HOA_DON_NGUOI_DUNG");

            entity.HasOne(d => d.Pttt).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.PtttId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HOA_DON_PHUONG_THUC_THANH_TOAN");

            entity.HasOne(d => d.Tthd).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.TthdId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HOA_DON_TRANG_THAI_HOA_DON");
        });

        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.LId).IsClustered(false);

            entity.ToTable("LOAI");

            entity.Property(e => e.LId).HasColumnName("L_ID");
            entity.Property(e => e.LTenloai)
                .HasMaxLength(50)
                .HasColumnName("L_TENLOAI");
        });

        modelBuilder.Entity<LoaiQuyen>(entity =>
        {
            entity.HasKey(e => e.LqId).IsClustered(false);

            entity.ToTable("LOAI_QUYEN");

            entity.Property(e => e.LqId).HasColumnName("LQ_ID");
            entity.Property(e => e.LqTenloaiquyen)
                .HasMaxLength(50)
                .HasColumnName("LQ_TENLOAIQUYEN");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.NdId).IsClustered(false);

            entity.ToTable("NGUOI_DUNG");

            entity.Property(e => e.NdId).HasColumnName("ND_ID");
            entity.Property(e => e.NdDiachi).HasColumnName("ND_DIACHI");
            entity.Property(e => e.NdGioitinh).HasColumnName("ND_GIOITINH");
            entity.Property(e => e.NdHinhanh)
                .IsUnicode(false)
                .HasColumnName("ND_HINHANH");
            entity.Property(e => e.NdHoten).HasColumnName("ND_HOTEN");
            entity.Property(e => e.NdSdt)
                .HasMaxLength(50)
                .HasColumnName("ND_SDT");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.NccId).IsClustered(false);

            entity.ToTable("NHA_CUNG_CAP");

            entity.Property(e => e.NccId).HasColumnName("NCC_ID");
            entity.Property(e => e.NccTen)
                .HasMaxLength(20)
                .HasColumnName("NCC_TEN");
        });

        modelBuilder.Entity<PhieuNhapHang>(entity =>
        {
            entity.HasKey(e => e.PnhId).IsClustered(false);

            entity.ToTable("PHIEU_NHAP_HANG");

            entity.Property(e => e.PnhId).HasColumnName("PNH_ID");
            entity.Property(e => e.NdId).HasColumnName("ND_ID");
            entity.Property(e => e.PndDongia)
                .HasColumnType("money")
                .HasColumnName("PND_DONGIA");
            entity.Property(e => e.PnhNgaynhap)
                .HasColumnType("datetime")
                .HasColumnName("PNH_NGAYNHAP");

            entity.HasOne(d => d.Nd).WithMany(p => p.PhieuNhapHangs)
                .HasForeignKey(d => d.NdId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PHIEU_NHAP_HANG_NGUOI_DUNG");
        });

        modelBuilder.Entity<PhuongThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.PtttId).IsClustered(false);

            entity.ToTable("PHUONG_THUC_THANH_TOAN");

            entity.Property(e => e.PtttId).HasColumnName("PTTT_ID");
            entity.Property(e => e.PtttTen)
                .HasMaxLength(50)
                .HasColumnName("PTTT_TEN");
        });

        modelBuilder.Entity<QuyenSuDung>(entity =>
        {
            entity.HasKey(e => e.QsdId).IsClustered(false);

            entity.ToTable("QUYEN_SU_DUNG");

            entity.HasIndex(e => e.LqId, "RELATIONSHIP_17_FK");

            entity.Property(e => e.QsdId).HasColumnName("QSD_ID");
            entity.Property(e => e.LqId).HasColumnName("LQ_ID");
            entity.Property(e => e.QsdTen)
                .HasMaxLength(50)
                .HasColumnName("QSD_TEN");

            entity.HasOne(d => d.Lq).WithMany(p => p.QuyenSuDungs)
                .HasForeignKey(d => d.LqId)
                .HasConstraintName("FK_QUYEN_SU_RELATIONS_LOAI_QUY");

            entity.HasMany(d => d.Vts).WithMany(p => p.Qsds)
                .UsingEntity<Dictionary<string, object>>(
                    "QuyensdVt",
                    r => r.HasOne<VaiTro>().WithMany()
                        .HasForeignKey("VtId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RELATION_RELATIONS_VAI_TRO"),
                    l => l.HasOne<QuyenSuDung>().WithMany()
                        .HasForeignKey("QsdId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RELATION_RELATIONS_QUYEN_SU"),
                    j =>
                    {
                        j.HasKey("QsdId", "VtId").HasName("PK_RELATIONSHIP_15");
                        j.ToTable("QUYENSD_VT");
                        j.HasIndex(new[] { "QsdId" }, "RELATIONSHIP_15_FK");
                        j.HasIndex(new[] { "VtId" }, "RELATIONSHIP_16_FK");
                        j.IndexerProperty<int>("QsdId")
                            .ValueGeneratedOnAdd()
                            .HasColumnName("QSD_ID");
                        j.IndexerProperty<int>("VtId").HasColumnName("VT_ID");
                    });
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.SpId).IsClustered(false);

            entity.ToTable("SAN_PHAM");

            entity.HasIndex(e => e.NccId, "CUNG_CAP_FK");

            entity.HasIndex(e => e.LId, "GOM_FK");

            entity.Property(e => e.SpId).HasColumnName("SP_ID");
            entity.Property(e => e.LId).HasColumnName("L_ID");
            entity.Property(e => e.NccId).HasColumnName("NCC_ID");
            entity.Property(e => e.SpGia)
                .HasColumnType("money")
                .HasColumnName("SP_GIA");
            entity.Property(e => e.SpHinhanh)
                .IsUnicode(false)
                .HasColumnName("SP_HINHANH");
            entity.Property(e => e.SpMota).HasColumnName("SP_MOTA");
            entity.Property(e => e.SpSoluongton).HasColumnName("SP_SOLUONGTON");
            entity.Property(e => e.SpTensp).HasColumnName("SP_TENSP");

            entity.HasOne(d => d.LIdNavigation).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.LId)
                .HasConstraintName("FK_SAN_PHAM_GOM_LOAI");

            entity.HasOne(d => d.Ncc).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.NccId)
                .HasConstraintName("FK_SAN_PHAM_CUNG_CAP_NHA_CUNG");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TkEmail).IsClustered(false);

            entity.ToTable("TAI_KHOAN");

            entity.HasIndex(e => e.NdId, "RELATIONSHIP_13_FK");

            entity.HasIndex(e => e.VtId, "RELATIONSHIP_14_FK");

            entity.Property(e => e.TkEmail)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TK_EMAIL");
            entity.Property(e => e.NdId).HasColumnName("ND_ID");
            entity.Property(e => e.TkMatkhau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TK_MATKHAU");
            entity.Property(e => e.VtId).HasColumnName("VT_ID");

            entity.HasOne(d => d.Nd).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.NdId)
                .HasConstraintName("FK_TAI_KHOA_RELATIONS_NGUOI_DU");

            entity.HasOne(d => d.Vt).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.VtId)
                .HasConstraintName("FK_TAI_KHOA_RELATIONS_VAI_TRO");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.TbId);

            entity.ToTable("THONG_BAO");

            entity.Property(e => e.TbId).HasColumnName("TB_ID");
            entity.Property(e => e.TbAvt).HasColumnName("TB_AVT");
            entity.Property(e => e.TbNguoigui).HasColumnName("TB_NGUOIGUI");
            entity.Property(e => e.TbNguoinhan).HasColumnName("TB_NGUOINHAN");
            entity.Property(e => e.TbNoidung).HasColumnName("TB_NOIDUNG");
            entity.Property(e => e.TbThoigian)
                .HasColumnType("datetime")
                .HasColumnName("TB_THOIGIAN");
            entity.Property(e => e.TbTieude)
                .HasMaxLength(50)
                .HasColumnName("TB_TIEUDE");
            entity.Property(e => e.TbTrangthai).HasColumnName("TB_TRANGTHAI");
        });

        modelBuilder.Entity<TrangThaiHoaDon>(entity =>
        {
            entity.HasKey(e => e.TthdId).IsClustered(false);

            entity.ToTable("TRANG_THAI_HOA_DON");

            entity.Property(e => e.TthdId).HasColumnName("TTHD_ID");
            entity.Property(e => e.TthdMota)
                .HasMaxLength(100)
                .HasColumnName("TTHD_MOTA");
            entity.Property(e => e.TthdTrangthai)
                .HasMaxLength(20)
                .HasColumnName("TTHD_TRANGTHAI");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.VtId).IsClustered(false);

            entity.ToTable("VAI_TRO");

            entity.Property(e => e.VtId).HasColumnName("VT_ID");
            entity.Property(e => e.VtTenvaitro)
                .HasMaxLength(50)
                .HasColumnName("VT_TENVAITRO");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System.ComponentModel.DataAnnotations;

namespace Quanlicuahang.Enum
{
    [Flags]
    public enum Permission
    {
        [Display(Name = "Không có quyền")]
        None = 0,

        [Display(Name = "Được xem dữ liệu")]
        canRead = 1 << 0,

        [Display(Name = "Được tạo mới")]
        canCreate = 1 << 1,

        [Display(Name = "Được chỉnh sửa")]
        canEdit = 1 << 2,

        [Display(Name = "Được xóa")]
        canDelete = 1 << 3,

        [Display(Name = "Được ngừng hoạt động")]
        canDeActive = 1 << 4,

        [Display(Name = "Được kích hoạt")]
        canActive = 1 << 5,

        [Display(Name = "Được xuất Excel")]
        canExportExcel = 1 << 6,

        [Display(Name = "Được nhập Excel")]
        canImportExcel = 1 << 7
    }
}

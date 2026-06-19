using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string Description { get; set; } = string.Empty;

        public List<UserRole> UserRoles { get; set; } = new();
    }

    public static class SystemRole
    {
        public const string User = "User";
        public const string Admin = "Admin";
    }
}
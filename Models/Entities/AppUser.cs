using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class AppUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu đã mã hóa không được để trống")]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<UserRole> UserRoles { get; set; } = new();

        public bool HasRole(string roleName)
        {
            return UserRoles.Any(ur =>
                ur.Role != null &&
                ur.Role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAdmin()
        {
            return HasRole(SystemRole.Admin);
        }

        public bool IsCustomer()
        {
            return HasRole(SystemRole.User);
        }
    }
}
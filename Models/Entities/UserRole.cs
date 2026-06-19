using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public AppUser? User { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        public Role? Role { get; set; }
    }
}
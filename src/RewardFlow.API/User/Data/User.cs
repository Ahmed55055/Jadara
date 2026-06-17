using RewardFlow_API.Common.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Reward_Flow_v2.User.Data
{
    public sealed class User : ITenantEntity
    {
        public int Id { get; private set; }
        public Guid UUID { get; private set; }
        public Guid TenantId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public UserRole? UserRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastVisit { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int PlanId { get; set; }
        public Plan? Plan { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        
        internal void PrepareNewUser(string username, string hashedPassword, string? email)
        {
            this.Username = username;
            this.PasswordHash = hashedPassword;
            this.Email = email;

            RoleId = ((int)UserRoleEnum.User);
            UserRole = null;
            CreatedAt = DateTime.Now;
            LastVisit = DateTime.Now;
            IsActive = true;
            ProfilePictureUrl = null;
            PlanId = ((int)PlanEnum.Free);
            Plan = null;
        }
    }
}
using AILifeAnalytics.Domain.Enums;

namespace AILifeAnalytics.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Activity> Activities { get; set; } = [];
        public ICollection<Insight> Insights { get; set; } = [];
        public UserSettings? Settings { get; set; }
    }
}

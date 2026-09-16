using System;

namespace Cdsqg.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// System, TaskReminder, OverdueAlert, StatusUpdate
        /// </summary>
        public string Type { get; set; } = "TaskReminder";

        public bool IsRead { get; set; } = false;

        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

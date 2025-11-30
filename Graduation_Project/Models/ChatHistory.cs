using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation_Project.Models
{
    public class ChatHistory
    {
        public int ChatHistoryId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        public string Message { get; set; }
        public string Reply { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

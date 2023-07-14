using System;

namespace Mirror.Models
{
    public class ScreenTime
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public DateTime Date { get; set; }
    }
}

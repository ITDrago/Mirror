using System;

namespace Mirror.Models
{
    public class Authorization
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public string Ip { get; set; }

        public DateTime Date { get; set; }

        public string Cookie { get; set; }

        public bool Deleted { get; set; }
    }
}

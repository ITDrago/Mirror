using System;

namespace Mirror.Models
{
    public class Request
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public string Address { get; set; }

        public DateTime Date { get; set; }

        public string Ip { get; set; }
    }
}

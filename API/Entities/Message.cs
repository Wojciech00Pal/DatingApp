using System;

namespace API.Entities
{
    public class Message
    {
        public int ID { get; set; }
        public int senderID { get; set; }
        public string senderUserName { get; set; }
        public AppUser sender { get; set; }
        public int recipientID { get; set; }
        public string recipientUserName { get; set; }
        public AppUser recipient { get; set; }
        public string content { get; set; }
        public DateTime? dateRead { get; set; }
        public DateTime? messageSent { get; set; } = DateTime.UtcNow;
        public bool senderDelted { get; set; }
        public bool recipientDelted { get; set; }
    }
}
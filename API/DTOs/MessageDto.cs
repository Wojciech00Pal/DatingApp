using System;
using API.Entities;

namespace API.DTOs
{
    public class MessageDto
    {
        public int ID { get; set; }
        public int senderID { get; set; }
        public string senderUserName { get; set; }
        public string senderPhotoUrl {get;set;}
        public int recipientID { get; set; }
        public string recipientUserName { get; set; }
        public string recipientPhotoUrl { get; set; }
        public string content { get; set; }
        public DateTime? dateRead { get; set; }
        public DateTime? messageSent { get; set; } 
    }
}
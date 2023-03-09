using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);

        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName);

        void AddGroup(Group_ group);

        void RemoveConnection(Connection connection);

        Task<Connection> GetConnection(string connectionId);
        Task<Group_> GetMessageGroup(string groupName);
        Task<Group_> GetGroupForConnection(string ConnectionId);
    }
}
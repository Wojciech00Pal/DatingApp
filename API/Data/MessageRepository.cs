using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group_ group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group_> GetGroupForConnection(string ConnectionId)
        {
            return await _context.Groups
                .Include(x=>x.Connections)
                .Where(x => x.Connections.Any(c=> c.ConnectionId == ConnectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group_> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                    .Include(x => x.Connections)
                    .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.messageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.recipientUserName == messageParams.Username && u.recipientDelted == false),
                "Outbox" => query.Where(u => u.senderUserName == messageParams.Username && u.senderDelted == false),
                _ => query.Where(u => u.recipientUserName == messageParams.Username &&
                 u.recipientDelted == false && u.dateRead == null)
                //defaault is Unread
            };
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName) //current ->zalogowany , recipient ->wysylajacy
        {
            var messages = await _context.Messages
                .Include(u => u.sender).ThenInclude(p => p.Photos)
                .Include(u => u.recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.recipientUserName == currentUserName && m.recipientDelted == false &&
                    m.senderUserName == recipientUserName || //or
                    m.recipientUserName == recipientUserName && m.senderDelted == false &&
                    m.senderUserName == currentUserName
                )
                .OrderBy(m => m.messageSent)//latest messages first
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.dateRead == null &&
            m.recipientUserName == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var mes in unreadMessages)
                {
                    mes.dateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);

        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            var ret = await _context.SaveChangesAsync();
            return ret >= 1;
        }
    }
}
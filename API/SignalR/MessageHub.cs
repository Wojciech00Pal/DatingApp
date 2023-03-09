using System.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {

        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IUnitOfWork _uow;

        public MessageHub(IUnitOfWork uow,
        IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
            _uow = uow;
            _presenceHub = presenceHub;
            _mapper = mapper;

        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            if(_uow.HasChanges()) await _uow.Complete();    

            await Clients.Caller.SendAsync("ReciveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.recipientUserName.ToLower())
            {
                throw new HubException("You cannnot send messages to yourself");
            }
            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.recipientUserName);

            if (recipient == null)
            {
                throw new HubException("Not found user");
            }

            var message = new Message()
            {
                sender = sender,
                recipient = recipient,
                senderUserName = sender.UserName,
                recipientUserName = recipient.UserName,
                content = createMessageDto.content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.UserName == recipient.UserName))// podlaczony do  connection user przeczytal
            {
                message.dateRead = DateTime.UtcNow;
            }
            else //obydwie osoby nie siedza na czacie razem
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessages", _mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private async Task<Group_> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var conn = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group_(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(conn);
            if (await _uow.Complete()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group_> RemoveFromMessageGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }
    }
}
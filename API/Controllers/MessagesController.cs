using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepsoitory;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepsoitory,
        IMessageRepository messageRepository,
        IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepsoitory = userRepsoitory;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage
        (CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.recipientUserName.ToLower())
            {
                return BadRequest("You cannot send messages to yourself");
            }
            var sender = await _userRepsoitory.GetUserByUsernameAsync(username);
            var recipient = await _userRepsoitory.GetUserByUsernameAsync(createMessageDto.recipientUserName);

            if (recipient == null)
            {
                return NotFound();
            }

            var message = new Message()
            {
                sender = sender,
                recipient = recipient,
                senderUserName = sender.UserName,
                recipientUserName = recipient.UserName,
                content = createMessageDto.content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
            return BadRequest("failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }
        
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await _messageRepository.GetMessageThread(currentUsername,username));
            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _messageRepository.GetMessage(id);

            if(message.senderUserName!=username && message.recipientUserName!=username)
            {
                return Unauthorized();
            }

            if(message.senderUserName == username)
            {
                message.senderDelted=true;
            }
            if(message.recipientUserName==username)
            {
                message.recipientDelted = true;
            }
            if(message.senderDelted &&message.recipientDelted)
            {
                _messageRepository.DeleteMessage(message);
            }
            if(await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
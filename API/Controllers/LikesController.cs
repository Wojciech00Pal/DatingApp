using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository _likesRepository;
        private readonly IUserRepository _userRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserID = User.GetUserId();
            var likedUser = _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = _likesRepository.GetUserWithLikes(sourceUserID).GetAwaiter().GetResult();

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepository.GetUserLike(sourceUserID, likedUser.Id);

            if (userLike != null) return BadRequest("You liked thas user before");

            userLike = new UserLike
            {
                SourceUserId = sourceUserID,
                TargetUserId = likedUser.Id
            };
               
            sourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SavaAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }
        [HttpGet]            
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserID = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
            users.PageSize,users.TotalCount,users.TotalPages));

            return Ok(users);
        }

    }
}
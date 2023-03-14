using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _dataContext;
        public PhotoRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<bool> CheckIfMainIsSet(int userId)
        {
            return await _dataContext.Photos
            .Where(x => x.AppUserId == userId)
            .AnyAsync(x => x.IsMain);
        }

        public async Task<Photo> GetPhotoById(int Id)
        {
            return await _dataContext.Photos
            .IgnoreQueryFilters()
            .Where(x => x.Id == Id).FirstOrDefaultAsync();
            //.SingleOrDefaultAsync(x => x.Id == Id) OR
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _dataContext.Photos
                .IgnoreQueryFilters()
                .Where(p => p.isApproved == false)
                .Select(u => new PhotoForApprovalDto
                {
                    Id = u.Id,
                    UserName = u.appUser.UserName,
                    Url = u.Url,
                    isApproved = u.isApproved
                }).ToListAsync();

        }

        public void RemovePhoto(Photo photo)
        {
            _dataContext.Photos.Remove(photo);
        }

    }
}
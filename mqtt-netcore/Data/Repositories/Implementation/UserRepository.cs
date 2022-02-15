using Data.Enumerations;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Implementation
{
   public class UserRepository :  RepositoryBase<User>, IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            item.DeletedAt = DateTime.Now;
            try
            {
                _context.Users.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteUserFromDatabase(int userId)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            try
            {
                _context.Users.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAllClientIdPrefixes()
        {
            var model = await _context.Users.Select(x => x.ClientidPrefix).ToListAsync();
            return model;
        }

        public async Task<IEnumerable<BlackList>> GetBlacklistItemsForUser(int userId, BlackList type)
        {
            var model = await _context.BlackList.Where(x => x.UserId == userId).ToListAsync();
            return model;
        }

        public async Task<IEnumerable<BlackList>> GetBlacklistItemsForUser(int userId, BlackListWhiteListType type)
        {
            return await _context.BlackList.Where(x => x.UserId == userId && x.Type.Equals(type)).ToListAsync();
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
        }

        public async Task<User> GetUserByName(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(x=> x.Username.Equals(userName));
        }

        public async Task<(string, int)> GetUserNameAndUserIdByName(string userName)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Username.Equals(userName));

            return (item.Username, item.Id);
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<IEnumerable<WhiteList>> GetWhitelistItemsForUser(int userId, BlackListWhiteListType type)
        {
            return await _context.WhiteList.Where(x => x.UserId == userId && x.Type.Equals(type)).ToListAsync();
        }

        public async Task<bool> InsertUser(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPassword(int userId, string hashedPassword)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            item.PasswordHash = hashedPassword;
            try
            {
                _context.Users.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                var item = await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(user.Id));
                if (item == null) return false;
                item.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserNameExists(string userName)
        {
            return await _context.Users.AnyAsync(x => x.Username.Equals(userName));
        }
    }
}

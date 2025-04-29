using Microsoft.EntityFrameworkCore;
using SurveySystem.Core.Interfaces;
using SurveySystem.Core.Models;
using SurveySystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveySystem.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsUserAdminAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            return user?.IsAdmin ?? false;
        }
    }
}
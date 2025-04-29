using SurveySystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SurveySystem.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> IsUserAdminAsync(Guid userId);
    }
}
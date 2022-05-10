using System.Linq.Expressions;
using PutAway.Shared.Entities;

namespace PutAway.Server.Services;

public interface IUserService
{

    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

    Task AssignDefaultRole(User user);

    Task<bool> UserExists(string email);


    Task<User> Add(User user);

    Task<User> AddAndSave(User user);

    Task<User?> GetByFilterIncludeRoles(Expression<Func<User, bool>> filter);

}
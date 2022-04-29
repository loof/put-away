using PutAway.Shared.Entities;

namespace PutAway.Server.Services;

public interface IUserService
{
    bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    
    void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

    Task AssignDefaultRole(User user);
}
using Code1.Models;

namespace Code1.Services
{
    public class UserService
    {
        public bool IsUserRegistered(string userId)
        {
            return RegisteredUsers.UserIds.Contains(userId);
        }
    }
}

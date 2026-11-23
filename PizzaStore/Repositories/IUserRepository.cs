using PizzaStore.Models;

namespace PizzaStore.Repositories
{
    public interface IUserRepository
    {
        void Register(User user);
        User? GetByEmail(string email);
    }
}
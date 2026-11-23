using System.Collections.Generic;
using PizzaStore.Models;

namespace PizzaStore.Repositories
{
    // Интерфейс определяет, *что* мы можем делать, но не говорит *как*. 
    // Это позволяет нам легко подменять реализацию в будущем (например, заменить SQL на фейковую базу для тестов).
    public interface IPizzaRepository
    {
        IEnumerable<Pizza> GetAllPizzas();
    }
}
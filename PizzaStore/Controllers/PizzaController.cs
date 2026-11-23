using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PizzaStore.Helpers;
using PizzaStore.Models;
using PizzaStore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaStore.Controllers
{
    public class PizzaController : Controller
    {
        private readonly IPizzaRepository _pizzaRepository;
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Database1;Trusted_Connection=True;TrustServerCertificate=True;";

        // Внедряем репозиторий пицц через конструктор
        public PizzaController(IPizzaRepository pizzaRepository)
        {
            _pizzaRepository = pizzaRepository;
        }

        // 1. Отображение каталога пицц
        public IActionResult Index()
        {
            var pizzas = _pizzaRepository.GetAllPizzas();
            return View(pizzas);
        }

        // 2. Добавление пиццы в корзину (через сессию)
        [HttpPost]
        public IActionResult AddToCart(int pizzaId, string pizzaName, decimal price, string imageUrl)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.PizzaId == pizzaId);
            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    PizzaId = pizzaId,
                    PizzaName = pizzaName,
                    Price = price,
                    Quantity = 1,
                    ImageUrl = imageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Cart");
        }

        // 3. Просмотр корзины
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // 4. Оформление заказа из корзины
        [HttpPost]
        public IActionResult Checkout(string deliveryTime, string paymentMethod)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            string userEmail = User.Identity.Name;
            string orderDate = DateTime.Now.ToString("yyyy-MM-dd");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                foreach (var item in cart)
                {
                    string query = "INSERT INTO Orders (UserEmail, PizzaName, Quantity, TotalPrice, DeliveryTime, OrderDate, PaymentMethod) VALUES (@Email, @Pizza, @Qty, @Total, @Time, @Date, @Pay)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", userEmail);
                        cmd.Parameters.AddWithValue("@Pizza", item.PizzaName);
                        cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                        cmd.Parameters.AddWithValue("@Total", item.TotalPrice);
                        cmd.Parameters.AddWithValue("@Time", deliveryTime);
                        cmd.Parameters.AddWithValue("@Date", DateTime.Parse(orderDate));
                        cmd.Parameters.AddWithValue("@Pay", paymentMethod);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Очищаем корзину после успешного оформления заказа
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("History");
        }

        // 5. Просмотр истории заказов клиента
        public IActionResult History()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            string userEmail = User.Identity.Name;
            var orders = new List<OrderViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT PizzaName, Quantity, TotalPrice, DeliveryTime, OrderDate, PaymentMethod, CreatedAt FROM Orders WHERE UserEmail = @Email ORDER BY CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", userEmail);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderViewModel
                            {
                                PizzaName = reader.GetString(0),
                                Quantity = reader.GetInt32(1),
                                TotalPrice = reader.GetDecimal(2),
                                DeliveryTime = reader.GetString(3),
                                OrderDate = reader.GetDateTime(4).ToString("dd.MM.yyyy"),
                                PaymentMethod = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6).ToString("dd.MM.yyyy HH:mm")
                            });
                        }
                    }
                }
            }

            return View(orders);
        }
    }
}
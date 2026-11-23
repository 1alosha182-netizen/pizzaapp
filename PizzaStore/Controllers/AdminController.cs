using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PizzaStore.Models;
using System;
using System.Collections.Generic;

namespace PizzaStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Database1;Trusted_Connection=True;TrustServerCertificate=True;";

        private bool IsAdmin()
        {
            if (!User.Identity.IsAuthenticated) return false;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Role WHERE Email = @Email"; // Или корректный запрос роли
                // Безопасная проверка роли:
                string roleQuery = "SELECT Role FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(roleQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", User.Identity.Name);
                    var role = cmd.ExecuteScalar()?.ToString();
                    return role == "Admin";
                }
            }
        }

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel();
            var orders = new List<AdminOrderViewModel>();
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Получаем все заказы за текущий день
                string orderQuery = "SELECT Id, UserEmail, PizzaName, Quantity, TotalPrice, DeliveryTime, OrderDate, PaymentMethod, Status, CreatedAt FROM Orders WHERE CAST(CreatedAt AS DATE) = @Today ORDER BY CreatedAt DESC";
                using (SqlCommand cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Today", today);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new AdminOrderViewModel
                            {
                                Id = reader.GetInt32(0),
                                UserEmail = reader.GetString(1),
                                PizzaName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                TotalPrice = reader.GetDecimal(4),
                                DeliveryTime = reader.GetString(5),
                                OrderDate = reader.GetDateTime(6).ToString("dd.MM.yyyy"),
                                PaymentMethod = reader.GetString(7),
                                Status = reader.GetString(8),
                                CreatedAt = reader.GetDateTime(9).ToString("HH:mm")
                            };

                            orders.Add(order);

                            // Считаем общую выручку и статистику по методам оплаты
                            viewModel.TotalRevenue += order.TotalPrice;

                            // Учитываем разные варианты названий методов оплаты в базе
                            string method = order.PaymentMethod?.Trim().ToLower() ?? "";
                            if (method.Contains("налич") || method.Contains("cash"))
                            {
                                viewModel.CashRevenue += order.TotalPrice;
                                viewModel.CashOrdersCount++;
                            }
                            else if (method.Contains("apple") || method.Contains("ap"))
                            {
                                viewModel.ApplePayRevenue += order.TotalPrice;
                                viewModel.ApplePayOrdersCount++;
                            }
                            else // По умолчанию считаем картой (Банковская карта и т.д.)
                            {
                                viewModel.CardRevenue += order.TotalPrice;
                                viewModel.CardOrdersCount++;
                            }
                        }
                    }
                }
                viewModel.Orders = orders;

                // 2. Проверяем состояние текущей смены
                string shiftQuery = "SELECT TOP 1 Id, OpenedAt, Status FROM Shifts WHERE Status = 'Open' ORDER BY OpenedAt DESC";
                using (SqlCommand cmd = new SqlCommand(shiftQuery, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            viewModel.ActiveShiftId = reader.GetInt32(0);
                            viewModel.ShiftOpenedAt = reader.GetDateTime(1).ToString("dd.MM.yyyy HH:mm");
                            viewModel.IsShiftOpen = true;
                        }
                        else
                        {
                            viewModel.IsShiftOpen = false;
                        }
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult OpenShift()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Shifts (AdminEmail, OpenedAt, Status) VALUES (@Email, @Time, 'Open')";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@Time", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CloseShift(int shiftId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE Shifts SET ClosedAt = @Time, Status = 'Closed' WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Time", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Id", shiftId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
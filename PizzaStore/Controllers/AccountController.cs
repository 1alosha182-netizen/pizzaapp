using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PizzaStore.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PizzaStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Database1;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Запрашиваем как Password, так и PasswordHash для универсальности
                string query = "SELECT ISNULL(Password, PasswordHash), Role FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email ?? "");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbPassword = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            string role = reader.IsDBNull(1) ? "Client" : reader.GetString(1);

                            // Проверка пароля
                            if (dbPassword == password)
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, email),
                                    new Claim(ClaimTypes.Role, role)
                                };

                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                var authProperties = new AuthenticationProperties
                                {
                                    IsPersistent = rememberMe // Сохранение куки при выборе "Запомнить меня"
                                };

                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                                if (role == "Admin")
                                {
                                    return RedirectToAction("Index", "Admin");
                                }
                                return RedirectToAction("Index", "Pizza");
                            }
                        }
                    }
                }
            }

            ModelState.AddModelError("", "Неверный email или пароль!");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Заполните все поля!");
                return View();
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Проверяем, существует ли уже такой пользователь
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    int userCount = (int)checkCmd.ExecuteScalar();
                    if (userCount > 0)
                    {
                        ModelState.AddModelError("", "Пользователь с таким email уже существует!");
                        return View();
                    }
                }

                // Регистрируем нового пользователя, записывая пароль в обе колонки (Password и PasswordHash) для совместимости
                string insertQuery = "INSERT INTO Users (Email, Password, PasswordHash, Role) VALUES (@Email, @Password, @Password, 'Client')";
                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.Parameters.AddWithValue("@Password", password);
                    insertCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
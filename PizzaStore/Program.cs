using PizzaStore.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Настройка аутентификации через куки
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    });

string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Database1;Trusted_Connection=True;TrustServerCertificate=True;";

// 🔴 ВОТ ЭТИ СТРОКИ ОБЯЗАТЕЛЬНО ДОЛЖНЫ БЫТЬ ДО builder.Build():
builder.Services.AddScoped<IPizzaRepository>(provider => new SqlPizzaRepository(connectionString));
builder.Services.AddScoped<IUserRepository>(provider => new SqlUserRepository(connectionString));
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication(); // Сначала аутентификация
app.UseAuthorization();  // Затем авторизация

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pizza}/{action=Index}/{id?}");

app.Run();

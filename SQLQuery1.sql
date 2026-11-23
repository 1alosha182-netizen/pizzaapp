-- Таблица пользователей (клиенты и администраторы)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL, -- В реальных проектах пароли хэшируются
    Role NVARCHAR(20) NOT NULL DEFAULT 'Client' -- 'Client' или 'Admin'
);

-- Таблица пицц
CREATE TABLE Pizzas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(255) NULL
);

-- Таблица заказов
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Принят', -- 'Принят', 'Готов к выдаче', 'Завершен'
    PaymentMethod NVARCHAR(50) NOT NULL, -- 'Карта онлайн', 'Наличные при получении', 'Карта курьеру'
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Связующая таблица для заказа и пицц (многие ко многим)
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT FOREIGN KEY REFERENCES Orders(Id),
    PizzaId INT FOREIGN KEY REFERENCES Pizzas(Id),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL
);
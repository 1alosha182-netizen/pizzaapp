-- Переходим к нужной базе данных
USE Database1;
GO

-- Удаляем старую таблицу, если она была повреждена или отсутствовала
DROP TABLE IF EXISTS Pizzas;

-- Создаем таблицу заново со всеми нужными колонкам
CREATE TABLE Pizzas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(500)
);
GO

-- Добавляем новый ассортимент пицц
INSERT INTO Pizzas (Name, Description, Price, ImageUrl) 
VALUES 
(N'Пепперони', N'Пикантная колбаса пепперони, моцарелла, фирменный томатный соус', 450.00, 'https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=600&q=80'),
(N'Маргарита', N'Классический итальянский рецепт: томаты, свежий базилик, моцарелла и оливковое масло', 390.00, 'https://images.unsplash.com/photo-1604382355076-af4b0eb60143?auto=format&fit=crop&w=600&q=80'),
(N'Четыре сыра', N'Изысканное сочетание моцареллы, горгонзолы, пармезана и нежного сливочного сыра', 520.00, 'https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80'),
(N'Барбекю', N'Сочный цыпленок, красный лук, бекон, соус барбекю и тягучий сыр моцарелла', 490.00, 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=600&q=80'),
(N'Мясной ураган', N'Пепперони, ветчина, охотничьи колбаски, бекон и фирменный томатный соус', 560.00, 'https://images.unsplash.com/photo-1541592106381-b31e9677c0e5?auto=format&fit=crop&w=600&q=80'),
(N'Гавайская', N'Сладкие кусочки ананаса, нежное куриное филе, моцарелла и фирменный соус', 440.00, 'https://images.unsplash.com/photo-1565299585323-38d6b0865b47?auto=format&fit=crop&w=600&q=80');

USE Database1;
GO

-- Если таблица Orders еще не создана вообще, создаем её
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' and xtype='U')
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserEmail NVARCHAR(100) NOT NULL,
        PizzaName NVARCHAR(100) NOT NULL,
        Quantity INT NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        DeliveryTime NVARCHAR(10) NOT NULL,
        OrderDate DATE NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
using System.Collections.Generic;

namespace PizzaStore.Models
{
    public class AdminDashboardViewModel
    {
        public List<AdminOrderViewModel> Orders { get; set; } = new List<AdminOrderViewModel>();
        public bool IsShiftOpen { get; set; }
        public int ActiveShiftId { get; set; }
        public string ShiftOpenedAt { get; set; }

        // Финансовая статистика за сегодня
        public decimal TotalRevenue { get; set; }         // Общая выручка
        public decimal CashRevenue { get; set; }          // Наличные
        public decimal CardRevenue { get; set; }          // Карты
        public decimal ApplePayRevenue { get; set; }      // Apple Pay

        // Количество заказов по методам оплаты
        public int CashOrdersCount { get; set; }
        public int CardOrdersCount { get; set; }
        public int ApplePayOrdersCount { get; set; }
    }

    public class AdminOrderViewModel
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string PizzaName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string DeliveryTime { get; set; }
        public string OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }
}
using Rafiq.Models;

namespace Rafiq.Models
{

    public enum OrderStatus
    {
        Pending,
        Paid,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? PaidAt { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
namespace OnlineBookStore.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Status { get; set; }
        public string? ClaimCode { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}

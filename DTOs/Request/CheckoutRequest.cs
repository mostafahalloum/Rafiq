namespace Rafiq.DTOs.Request
{
    public class CheckoutRequest
    {
        public string Address { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}

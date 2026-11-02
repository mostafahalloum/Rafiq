namespace Rafiq.Models
{
    public class ApplicationUserOTP
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OTP { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddMinutes(5);
        public bool IsValid { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser? ApplicationUser { get; set; }
    }
}

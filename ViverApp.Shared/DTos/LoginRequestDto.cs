namespace ViverApp.Shared.Dtos
{
    public class LoginRequestDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int UserType { get; set; }
        public string? Devicetoken { get; set; }
    }
}

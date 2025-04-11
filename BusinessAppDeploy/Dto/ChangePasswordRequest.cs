namespace Business.Dto
{
    public class ChangePasswordRequest
    {
        public string? Token { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}

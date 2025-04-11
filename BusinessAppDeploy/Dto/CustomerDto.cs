namespace Business.Dto
{
    public class CustomerDto
    {
        public int Cus_Id { get; set; }
        public string? Cus_EmailId { get; set; }
        public string? Cus_Password { get; set; }
        public string? Cus_Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? pinCode { get; set; }
    }
}

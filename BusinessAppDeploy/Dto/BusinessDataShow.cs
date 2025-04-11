namespace Business.Dto
{
    public class BusinessDataShow
    {
        public int BusinessID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double? Distancekm { get; set; }
        public double? Latitude { get; set; }
        public double? longitude { get; set; }
        public string? VisitingCard { get; set; }
        public string? Location { get; set; }
        public decimal? AverageRating { get; set; }
        public int RoleID { get; set; }
}
}

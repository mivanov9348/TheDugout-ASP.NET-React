namespace TheDugout.Models.Staff
{
    public class AgencyTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string RegionCode { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}

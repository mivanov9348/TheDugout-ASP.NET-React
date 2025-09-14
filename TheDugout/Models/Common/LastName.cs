namespace TheDugout.Models.Common
{
        public class LastName
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string RegionCode { get; set; } = null!;
        public Region Region { get; set; } = null!;
    }
}

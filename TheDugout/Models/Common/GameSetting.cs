namespace TheDugout.Models.Common
{
    using System.ComponentModel.DataAnnotations;
    public class GameSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = null!;
    }
}

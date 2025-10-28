namespace TheDugout.Models.Common
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using TheDugout.Models.Enums;
    public class GameSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = null!;

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameSettingCategory Category { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

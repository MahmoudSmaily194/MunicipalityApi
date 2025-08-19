using SawirahMunicipalityWeb.Enums;

namespace SawirahMunicipalityWeb.Models
{
    public class MunicipalServiceResponceDto
    {
        public Guid Id { get; set; } 
        public string? ImageUrl { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Slug { get; set; }

        // Fk
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}

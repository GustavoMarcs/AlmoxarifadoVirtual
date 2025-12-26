using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public abstract class EntityBase
{
    [Key]
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
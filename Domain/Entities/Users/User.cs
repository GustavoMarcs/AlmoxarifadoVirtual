namespace Domain.Entities.Users;

public sealed class User : EntityBase
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string Role { get; set; }
}

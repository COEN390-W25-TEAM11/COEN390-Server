namespace Infrastructure.Entity;

public class User {
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool IsEnabled { get; set; }
    public required bool IsAdmin { get; set; }
}


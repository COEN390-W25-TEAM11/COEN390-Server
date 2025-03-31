namespace Infrastructure.Entity;

public class Light {
    public required Guid Id { get; set; }
    public required Guid EspId { get; set; }
    public required int Pin { get; set; }
    public required string Name { get; set; }

    public required bool Overide { get; set; }
    public required int State { get; set; }
    public required int Brightness { get; set; }

    public required List<Assigned> assigned { get; set;}
}

﻿namespace Infrastructure.Entity;

public class Light {
    public Guid Id { get; set; }
    public Guid EspId { get; set; }
    public int Pin { get; set; }
    public string Name { get; set; }

    public bool Overide { get; set; }
    public int State { get; set; }
    public int Brightness { get; set; }

    public List<Assigned> assigned { get; set;}
}

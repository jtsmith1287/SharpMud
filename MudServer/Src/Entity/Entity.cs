using System;

namespace MudServer.Entity {
public class Entity {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public override string ToString() {
        return $"{Name} ({Id})";
    }
}
}

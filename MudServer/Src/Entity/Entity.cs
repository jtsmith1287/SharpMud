using System;

namespace MudServer.Entity {
    public abstract class Entity {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }
        public bool Secret { get; set; }

        public override string ToString() {
            return $"{Name} ({Id})";
        }
    }
}

using System;
using System.Collections.Generic;
using MudServer.Interfaces;

namespace MudServer.Entity {
    public class ItemDef {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsContainer { get; set; }
        public bool Takeable { get; set; }
        public bool Equippable { get; set; }
        public bool Hidden { get; set; }
        public bool Secret { get; set; }
        public List<IItemAction> OnUse { get; set; } = new List<IItemAction>();
    }
}

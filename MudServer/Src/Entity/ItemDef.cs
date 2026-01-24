using System;
using System.Collections.Generic;
using MudServer.Interfaces;
using MudServer.Actions;

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
    public bool Unique { get; set; }
    public List<ItemActionDef> OnUse { get; set; } = new List<ItemActionDef>();
    public static Dictionary<string, ItemDef> Registry { get; set; } = new Dictionary<string, ItemDef>();
}

public class ItemActionDef {
    public string ActionType { get; set; }
    public string TargetId { get; set; }
    public string CreatureId { get; set; }
}
}

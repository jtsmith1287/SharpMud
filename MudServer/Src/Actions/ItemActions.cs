using MudServer.Entity;
using MudServer.Interfaces;
using MudServer.World;

namespace MudServer.Actions {
public class RevealItemAction : IItemAction {
    public string TargetId { get; set; }

    public void Execute(IActor actor, Item item) {
        // Example: drawer reveals note
        if (actor is IBroadcastable broadcastable) {
            broadcastable.SendToClient($"You pull the lever and hear a click from the {TargetId}!", Util.Color.Cyan);
        }
    }
}

public class SpawnCreatureAction : IItemAction {
    public string CreatureId { get; set; }

    public void Execute(IActor actor, Item item) {
        // Example: a cursed chest spawns a skeleton
    }
}

public class AddToInventoryAction : IItemAction {
    public void Execute(IActor actor, Item item) {
        // Example: picking up the note or weapon
    }
}

public class EquipItemAction : IItemAction {
    public void Execute(IActor actor, Item item) {
        // Example: equipping a rapier
    }
}
}

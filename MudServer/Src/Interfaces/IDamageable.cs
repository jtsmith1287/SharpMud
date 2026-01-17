using System;
using MudServer.Entity;
using MudServer.Enums;

namespace MudServer.Interfaces {
    /// <summary>
    /// Handles HP tracking and death.
    /// </summary>
    public interface IDamageable {
        int Health { get; set; }
        void ApplyDamage(int amount);
    }

    /// <summary>
    /// Handles actor interactions, such as combat, movement, social interactions, leveling, etc. 
    /// </summary>
    public interface IActor {
        Stats Stats { get; set; }
        GameState GameState { get; set; }
    }

    /// <summary>
    /// Handles the connection and broadcasting of messages. Commonly just a separation of players from NPCs.
    /// </summary>
    public interface IBroadcastable {
        void SendToClient(string message, string color = "");
        void BroadcastLocal(string message, string color = "", params Guid[] ignore);
    }

    /// <summary>
    /// This object can be equipped or worn.
    /// </summary>
    public interface IWearable { }

    /// <summary>
    /// This object can be interacted with.
    /// </summary>
    public interface IUsable {
        void Use(IActor actor);
    }

    public interface ITakeable {
        void Take(IActor actor);
    }

    /// <summary>
    /// This object can contain other objects, such as containers or rooms.
    /// </summary>
    public interface IContainer { }

    /// <summary>
    /// This object represents a room in the game world.
    /// </summary>
    public interface IRoom { }

    public interface IItemAction {
        void Execute(IActor actor, Item item);
    }
}

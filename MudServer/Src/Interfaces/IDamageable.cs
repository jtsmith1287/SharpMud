namespace MudServer.Interfaces {
/// <summary>
/// Handles HP tracking and death.
/// </summary>
public interface IDamageable { }

/// <summary>
/// Handles actor interactions, such as combat, movement, social interactions, leveling, etc. 
/// </summary>
public interface IActor { }

/// <summary>
/// Handles the connection and broadcasting of messages. Commonly just a separation of players from NPCs.
/// </summary>
public interface IBroadcastable { }

/// <summary>
/// This object can be equipped or worn.
/// </summary>
public interface IWearable { }

/// <summary>
/// This object can be interacted with.
/// </summary>
public interface IUsable { }

/// <summary>
/// This object can contain other objects, such as containers or rooms.
/// </summary>
public interface IContainer { }

/// <summary>
/// This object represents a room in the game world.
/// </summary>
public interface IRoom { }
}

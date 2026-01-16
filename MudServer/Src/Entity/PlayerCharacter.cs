using MudServer.Interfaces;

namespace MudServer.Entity {
/// <summary>
/// Represents an instance of a player in the game.
/// </summary>
public class PlayerCharacter : Entity, IActor, IDamageable, IBroadcastable { }

/// <summary>
/// Represents an instance of a non-player character in the game, such as a vendor, or monster.
/// </summary>
public class NonPlayerCharacter : Entity, IActor, IDamageable { }
}

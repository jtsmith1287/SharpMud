namespace MudServer.World {
public class Exit {
    /// <summary>
    /// Represents the path of the exit, with two coordinates defining the start and end points.
    /// <remarks>There is no "direction". Order is arbitrary and just a representation of the connected rooms.</remarks>
    /// </summary>
    public Coordinate3[] Path = new Coordinate3[2];
    public bool Locked { get; set; }
    public bool Open { get; set; } = true;
    /// <summary>
    /// True if the exit is functionally nonexistent until a certain condition is met. Not displayed to players.
    /// <remarks>Indexes should match that of Path. Exits can be hidden from one direction, but not the other.</remarks>
    /// <example>[true, false] would mean entities in the room at Path[0] cannot see the exit to Path[1].</example>
    /// </summary>
    public bool[] Hidden = { false, false };
    /// <summary>
    /// Similar to Hidden, but the exit is functional. Secret simply means it is not listed as an exit but otherwise
    /// works normally.
    /// </summary>
    public bool[] Secret = { false, false };
}
}

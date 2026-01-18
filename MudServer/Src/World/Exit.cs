using System;
using System.Linq;

namespace MudServer.World {
public class Exit {
    /// <summary>
    /// Returns a unique identifier for the exit based on its path.
    /// Since exits are bidirectional, the path is sorted to ensure a consistent ID.
    /// </summary>
    public string GetPathId() {
        if (Path == null || Path.Length < 2 || Path[0] == null || Path[1] == null) return string.Empty;
        
        Coordinate3 first = Path[0];
        Coordinate3 second = Path[1];
        
        // Sort coordinates to ensure stability
        if (Compare(first, second) > 0) {
            first = Path[1];
            second = Path[0];
        }
        
        return $"{first.X}_{first.Y}_{first.Z}_{second.X}_{second.Y}_{second.Z}";
    }

    private int Compare(Coordinate3 a, Coordinate3 b) {
        if (a.X != b.X) return a.X.CompareTo(b.X);
        if (a.Y != b.Y) return a.Y.CompareTo(b.Y);
        return a.Z.CompareTo(b.Z);
    }

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
    public readonly bool[] Hidden = { false, false };
    /// <summary>
    /// Similar to Hidden, but the exit is functional. Secret simply means it is not listed as an exit but otherwise
    /// works normally.
    /// </summary>
    public readonly bool[] Secret = { false, false };

    public bool IsInPath(Coordinate3 coordinate) {
        return Path.Contains(coordinate);
    }

    public bool IsHidden(Coordinate3 from) {
        return IsInPath(from) && Hidden[Array.IndexOf(Path, from)];
    }
    
    public bool IsSecret(Coordinate3 from) {
        return IsInPath(from) && Secret[Array.IndexOf(Path, from)];
    }
}
}

using System;
using System.Text;
using MudServer.Util;

namespace MudServer.World {
public static class AnsiMap {
    public static string Display(Coordinate3 location, int radius = 6) {
        if (location == null) return string.Empty;

        StringBuilder sb = new StringBuilder();
        int minX = location.X - radius;
        int maxX = location.X + radius;
        int minY = location.Y - radius;
        int maxY = location.Y + radius;

        for (int y = maxY; y >= minY; y--) {
            // Room row
            StringBuilder roomRow = new StringBuilder();
            // Connection row (for vertical connections between rooms)
            StringBuilder connRow = new StringBuilder();

            for (int x = minX; x <= maxX; x++) {
                Coordinate3 currentPos = new Coordinate3(x, y, location.Z);
                if (!World.TryGetRoom(currentPos, out Room room)) {
                    roomRow.Append("   ");
                    if (y > minY) {
                        connRow.Append("   ".Substring(0, x < maxX ? 3 : 1));
                    }
                    continue;
                }

                bool isCurrent = (x == location.X && y == location.Y);

                // Room representation
                string roomColor = isCurrent ? Color.Green : Color.White;
                string roomSymbol = "#";

                // Check for Up/Down exits
                bool hasUp = false;
                bool hasDown = false;
                foreach (var exit in room.ConnectedRooms.Values) {
                    if (exit.Z > room.Location.Z) hasUp = true;
                    if (exit.Z < room.Location.Z) hasDown = true;
                }

                if (hasUp && hasDown) roomSymbol = "B"; // Both
                else if (hasUp) roomSymbol = "U";
                else if (hasDown) roomSymbol = "D";

                roomRow.Append(roomColor + roomSymbol + Color.Reset);

                // East connection
                if (x < maxX) {
                    Coordinate3 eastPos = new Coordinate3(x + 1, y, location.Z);
                    if (!World.TryGetRoom(eastPos, out Room eastRoom) || !HasConnection(room, eastRoom)) {
                        roomRow.Append("  ");
                    } else {
                        roomRow.Append(Color.Yellow + "--" + Color.Reset);
                    }
                }

                // South connection
                if (y > minY) {
                    Coordinate3 southPos = new Coordinate3(x, y - 1, location.Z);
                    if (!World.TryGetRoom(southPos, out Room southRoom) || !HasConnection(room, southRoom)) {
                        connRow.Append("   ".Substring(0, x < maxX ? 3 : 1));
                    } else {
                        connRow.Append(Color.Yellow + "|" + Color.Reset + (x < maxX ? "  " : ""));
                    }
                }
            }

            sb.AppendLine(roomRow.ToString());
            if (connRow.Length > 0) {
                sb.AppendLine(connRow.ToString());
            }
        }

        return sb.ToString();
    }

    private static bool HasConnection(Room a, Room b) {
        bool aToB = false;
        foreach (var exit in a.ConnectedRooms.Values) {
            if (exit == b.Location) {
                aToB = true;
                break;
            }
        }

        bool bToA = false;
        foreach (var exit in b.ConnectedRooms.Values) {
            if (exit == a.Location) {
                bToA = true;
                break;
            }
        }

        return aToB && bToA;
    }
}
}

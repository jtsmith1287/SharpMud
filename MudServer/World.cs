using System;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	public static class World {
		
		public static Dictionary<string, Room> Rooms = new Dictionary<string, Room> ();

		public static bool AddRoom (Room room) {
		
			string stringCoord = string.Format ("{0} {1} {2}", room.Location.X, room.Location.Y, room.Location.Z);
			
			// Check if the room exists already and return false if it does.
			if (Rooms.TryGetValue (stringCoord, out room)) {
				return false;
			}
			Rooms.Add (stringCoord, room);
			return true;
		}

		public static Room GetRoom (Coordinate3 location) {
		
			Room room;
			string stringCoord = string.Format ("{0} {1} {2}", location.X, location.Y, location.Z);
			Rooms.TryGetValue (stringCoord, out room);
			return room;
			
		}
	}
}


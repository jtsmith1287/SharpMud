using System;
using System.Threading;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	public static class World {

		public static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
		public static Dictionary<Guid, Mobile> Mobiles = new Dictionary<Guid, Mobile>();
		public static List<Thread> SpawnThreads = new List<Thread>();

		public static bool AddRoom(Room room) {

			string stringCoord = string.Format("{0} {1} {2}", room.Location.X, room.Location.Y, room.Location.Z);

			// Check if the room exists already and return false if it does.
			Room maybeRoom;
			if (Rooms.TryGetValue(stringCoord, out maybeRoom)) {
				return false;
			}
			Rooms.Add(stringCoord, room);
			return true;
		}

		public static Room GetRoom(Coordinate3 location) {

			if (location == null) {
				return null;
			}
			Room room;
			string stringCoord = string.Format("{0} {1} {2}", location.X, location.Y, location.Z);
			Rooms.TryGetValue(stringCoord, out room);
			return room;
		}

		/// <summary>
		/// Gets the room by ID. Incredibly slow, avoid using.
		/// </summary>
		/// <returns>The room by ID.</returns>
		/// <param name="id">Identifier.</param>
		public static Room GetRoomByID(Guid id) {

			foreach (var entry in Rooms) {
				if (entry.Value.ID == id) {
					return entry.Value;
				}
			}
			return null;
		}

		internal static void StopAllSpawnThreads() {

			foreach (Thread thread in SpawnThreads) {
				thread.Abort();
			}
		}
	}
}


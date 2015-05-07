using System;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	public class Room {

		public Coordinate3 Location;
		public string Name;
		Dictionary<Direction, Room> ConnectedRooms = new Dictionary<Direction, Room> ();
		public List<Guid> EntitiesHere = new List<Guid> ();

		public Room (Coordinate3 location, string name) {
			
			Location = location;
			Name = name;
			World.AddRoom (this);

		}

		public Room GetConnectedRoom (Direction direction) {
		
			Room room;
			ConnectedRooms.TryGetValue (direction, out room);
			
			// Can be null
			return room;
		}
	}
}


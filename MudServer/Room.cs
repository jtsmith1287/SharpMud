using System;
using System.Collections.Generic;
using GameCore.Util;

namespace GameCore {
	[Serializable]
	public class Room {

		public Coordinate3 Location;
		public string Name;
		public List<Guid> ConnectedRooms = new List<Guid> ();
		public List<Guid> EntitiesHere = new List<Guid> ();
		public Guid ID;

		public Room (Coordinate3 location, string name) {
			
			Location = location;
			Name = name;
			ID = Guid.NewGuid ();
			World.AddRoom (this);
		}
	}
}


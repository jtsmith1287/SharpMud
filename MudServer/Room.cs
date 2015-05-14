using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GameCore.Util;

namespace GameCore {
	[Serializable]
	public class Room {

		public Coordinate3 Location;
		public string Name;
		public string Description;
		public List<Guid> ConnectedRooms = new List<Guid> ();
		[NonSerialized]
		public List<Guid> EntitiesHere = new List<Guid> ();
		public List<Spawner> SpawnersHere = new List<Spawner> ();
		public Guid ID;

		public Room (Coordinate3 location, string name) {

			Location = location;
			Name = name;
			ID = Guid.NewGuid ();
			World.AddRoom (this);
		}

		[OnDeserializing]
		void InitializeEntitiesList (StreamingContext c) {

			EntitiesHere = new List<Guid> ();
		}
	}
}


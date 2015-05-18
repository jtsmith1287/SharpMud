using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GameCore.Util;

namespace GameCore {
	[Serializable]
	public class Room {

		[NonSerialized]
		public static Dictionary<string, string> DirectionExtensions = new Dictionary<string, string> { 
			{"n", "north"},
			{"s", "south"},
			{"e", "east"},
			{"w", "west"},
			{"d", "down"},
			{"u", "up"}
		};

		public Coordinate3 Location;
		public string Name;
		public string Description;
		public List<Guid> ConnectedRooms = new List<Guid>();
		[NonSerialized]
		public List<Guid> EntitiesHere = new List<Guid>();
		public List<Spawner> SpawnersHere = new List<Spawner>();
		public Guid ID;

		public Room(Coordinate3 location, string name) {

			Location = location;
			Name = name;
			ID = Guid.NewGuid();
			World.AddRoom(this);
		}

		public string GetDirection(Coordinate3 destination) {

			Coordinate3 diff = Location - destination;

			if (diff.X == 1)
				return "west";
			if (diff.X == -1)
				return "east";
			if (diff.Y == 1)
				return "south";
			if (diff.Y == -1)
				return "north";
			if (diff.Z == 1)
				return "down";
			if (diff.Z == -1)
				return "up";
			else
				return "";
		}

		[OnDeserializing]
		void InitializeEntitiesList(StreamingContext c) {

			EntitiesHere = new List<Guid>();
		}
	}
}


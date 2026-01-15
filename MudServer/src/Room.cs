using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using GameCore.Util;

namespace GameCore {
	public class Room {

		public static Dictionary<string, string> DirectionNames = new Dictionary<string, string>(){
			{"n", "north"},
			{"s", "south"},
			{"e", "east"},
			{"w", "west"},
			{"u", "up"},
			{"d", "down"},
		};
		public static Dictionary<string, Coordinate3> DirectionMap = new Dictionary<string, Coordinate3>(){
			{"n", new Coordinate3(0, 1, 0)},
			{"s", new Coordinate3(0, -1, 0)},
			{"e", new Coordinate3(1, 0, 0)},
			{"w", new Coordinate3(-1, 0, 0)},
			{"u", new Coordinate3(0, 0, 1)},
			{"d", new Coordinate3(0, 0, -1)},
		};

		public Coordinate3 Location;
		public string Name;
		public string Description;
		public string MapName;
		public Dictionary<string, Coordinate3> ConnectedRooms = new Dictionary<string, Coordinate3>();
		public Dictionary<string, Coordinate3> InvisibleConnections = new Dictionary<string, Coordinate3>();
		public bool IsEntryRoom = false;
		[ScriptIgnore]
		public List<Guid> EntitiesHere = new List<Guid>();
		public List<Spawner> SpawnersHere = new List<Spawner>();
		public Guid ID;

		public Room() {
			EntitiesHere = new List<Guid>();
		}

		public Room(Coordinate3 location, string name) : this() {

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
	}
}


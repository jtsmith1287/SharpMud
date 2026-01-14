using System;

namespace GameCore.Util {
	public enum Disposition {
		Friendly,
		Hostile,
		Neutral,
	}

	public enum Group {
		Player,
		Orc,
		Goblin,
		Spider,
		Wyrm,
		Dragon,
		Undead,
		Werewolf,
		Villager,
		Guard,
	}

	public struct DataPaths {
		public const string UserPwd = "userpwd.json";
		public const string UserId = "userid.json";
		public const string IdData = "iddata.json";
		public const string World = "world.json";
		public const string MapList = "maps\\maps.json";
		public const string Spawn = "spawn.json";
	}
}


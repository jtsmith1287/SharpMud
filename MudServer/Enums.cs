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
	}

	public struct DataPaths {
		public const string UserPwd = "userpwd";
		public const string UserId = "userid";
		public const string IdData = "iddata";
		public const string World = "world";
		public const string Spawn = "spawn";
	}
}


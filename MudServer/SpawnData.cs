using System;

namespace GameCore.Util {
	/// <summary>
	/// Contains data to construct a spawn. This is to be passed to a spawner which will generate a Mobile using the data.
	/// </summary>
	[Serializable]
	public class SpawnData : Data {

		public Disposition Behaviour;
		public Group Faction;

		public SpawnData() {

			// For Testing only
			Behaviour = Disposition.Hostile;
			Faction = Group.Orc;
			Name = "Orc";
			Level = 1;
			Health = 8;
			MaxHealth = 8;
		}

		SpawnData(string name, Guid id)
			: base(name, id) {
		}
	}
}


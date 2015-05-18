using System;

namespace GameCore.Util {
	/// <summary>
	/// Contains data to construct a spawn. This is to be passed to a spawner which will generate a Mobile using the data.
	/// </summary>
	[Serializable]
	public class SpawnData : Data {

		public Disposition Behaviour { get; set; }
		public Group Faction { get; set; }
		public bool Humanoid = true;

		public SpawnData(string name) {

			Name = name;
			Level = 1;
		}

		SpawnData(string name, Guid id)
			: base(name, id) {
		}
	}
}


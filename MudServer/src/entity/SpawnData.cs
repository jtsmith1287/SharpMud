using System;

namespace GameCore.Util {
	/// <summary>
	/// Contains data to construct a spawn. This is to be passed to a spawner which will generate a Mobile using the data.
	/// </summary>
	public class SpawnData : Stats {

		public Disposition Behaviour { get; set; }
		public Taxonomy Faction { get; set; }
		public bool Humanoid { get; set; }

		public SpawnData() {
			Humanoid = true;
		}

		public SpawnData(string name) : this() {

			Name = name;
			Level = 1;
		}

		SpawnData(string name, Guid id)
			: base(name, id) {
			Humanoid = true;
		}
	}
}


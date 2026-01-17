using System;

using System;
using MudServer.Enums;

namespace MudServer.Entity {
	/// <summary>
	/// Contains data to construct a spawn. This is to be passed to a spawner which will generate a NonPlayerCharacter using the data.
	/// </summary>
	public class SpawnData : Stats {

		public Disposition Behaviour { get; set; }
		public Taxonomy Faction { get; set; }
		public bool Humanoid { get; set; }

		public SpawnData() {
			Humanoid = true;
			Id = Guid.NewGuid();
		}

		public SpawnData(string name) : this() {

			Name = name;
			Level = 1;
		}

		public override string ToString() {
			return $"{Name} ({Level})";
		}

		SpawnData(string name, Guid id)
			: base(name, id) {
			Humanoid = true;
		}
	}
}


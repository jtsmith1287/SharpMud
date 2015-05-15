using System;
using GameCore.Util;

namespace GameCore {

	public class Mobile : BaseMobile {

		public Spawner SpawnerParent;

		public Mobile(SpawnData data, Spawner spawner) {

			Name = data.Name;
			Stats = data;
			SpawnerParent = spawner;
			GenerateID();
		}

		public void ExecuteLogic() {

			Console.WriteLine("I am a " + Name + " and I'm alive...");
		}

		void OnDeath() {

			SpawnerParent.Remove(this);
			World.Mobiles.Remove(ID);
		}
	}
}


using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GameCore.Util {
	[Serializable]
	public class Spawner {

		Guid ID;
		SpawnData[] Spawns;
		[NonSerialized]
		bool m_Spawning;

		public Spawner (Room room, params SpawnData[] spawnData) {
			
			Spawns = spawnData;
			room.SpawnersHere.Add (this);
			ID = Guid.NewGuid ();
			m_Spawning = true;
			StartSpawnThread ();
		}

		[OnDeserializing]
		internal void StartSpawnThread () {
			
			m_Spawning = true;
			Thread thread = new Thread (SpawnThread);
			World.SpawnThreads.Add (thread);
			thread.Start ();
		}

		void SpawnThread () {
			
			while (m_Spawning) {
				foreach (SpawnData data in Spawns) {
					Console.Write ("Spawning " + data.Name + " ");
				}
				Console.WriteLine ();
				Thread.Sleep (1000);
			}
		}
	}
}


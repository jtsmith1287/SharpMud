using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GameCore.Util {
	[Serializable]
	public class Spawner {

		Guid ID;
		SpawnData[] m_SpawnData;
		[NonSerialized]
		List<Mobile> m_Spawns = new List<Mobile>();
		int MaxNumberOfSpawn = 3;
		bool m_Spawning = false;
		Coordinate3 m_Location;


		public Spawner(Room room, params SpawnData[] spawnData) {

			m_SpawnData = spawnData;
			room.SpawnersHere.Add(this);
			m_Location = room.Location;
			ID = Guid.NewGuid();
			m_Spawning = true;
			StartSpawnThread(new StreamingContext());
		}

		[OnDeserialized]
		internal void StartSpawnThread(StreamingContext context) {

			m_Spawning = true;
			m_Spawns = new List<Mobile>();
			Thread thread = new Thread(SpawnThread);
			World.SpawnThreads.Add(thread);
			thread.Start();
		}

		void SpawnThread() {

			Random rand = new Random();
			int spawnPause = 5000;
			int spawnTimer = DateTime.Now.Millisecond + spawnPause;
			Console.WriteLine(spawnTimer);

			while (m_Spawning) {
				Console.WriteLine(DateTime.Now.Millisecond);
				if (m_Spawns.Count < MaxNumberOfSpawn) {
					if (spawnTimer < DateTime.Now.Millisecond) {
						Mobile newMob = new Mobile(m_SpawnData[rand.Next(m_SpawnData.Length)], this);
						m_Spawns.Add(newMob);
						newMob.Move(m_Location);
						spawnTimer = DateTime.Now.Millisecond + spawnPause;
					}
				}
				foreach (Mobile mob in m_Spawns) {
					mob.ExecuteLogic();
				}
				Thread.Sleep(1000);
			}
		}

		internal void Remove(Mobile mobile) {

			m_Spawns.Remove(mobile);
		}
	}
}


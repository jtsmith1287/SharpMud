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
		List<Mobile> m_DeadSpawn = new List<Mobile>();
		int MaxNumberOfSpawn = 3;
		bool m_Spawning = false;
		Coordinate3 m_Location;

		public Spawner(Room room, List<SpawnData> spawnList) {

			m_SpawnData = spawnList.ToArray();
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
			m_DeadSpawn = new List<Mobile>();
			Thread thread = new Thread(SpawnThread);
			World.SpawnThreads.Add(thread);
			thread.Start();
		}

		void SpawnThread() {

			Random rand = new Random();
			DateTime spawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));

			while (m_Spawning) {
				if (m_Spawns.Count < MaxNumberOfSpawn) {
					if (spawnTime < DateTime.Now) {
						Mobile newMob = new Mobile(m_SpawnData[rand.Next(0, m_SpawnData.Length)], this);
						m_Spawns.Add(newMob);
						newMob.Stats.OnZeroHealth += QueueDestroyMob;
						newMob.Move(m_Location);
						World.Mobiles.Add(newMob.ID, newMob);

						spawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 5));
					}
				}
				foreach (Mobile mob in m_Spawns) {
					mob.ExecuteLogic();
				}

				lock (m_DeadSpawn) {
					foreach (Mobile mob in m_DeadSpawn) {
						DestroyMob(mob);
					}
				}

				// Regulates AI logic to roughly 30 ticks a second.
				Thread.Sleep(33);
			}
		}

		private void DestroyMob(Mobile mob) {

			m_Spawns.Remove(mob);
			Console.WriteLine(mob.Name + " has died");
		}

		private void QueueDestroyMob(Data data) {

			foreach (Mobile spawn in m_Spawns) {
				if (spawn.ID == data.ID) {
					m_DeadSpawn.Add(spawn);
				}
			}
		}

		internal void Remove(Mobile mobile) {

			m_Spawns.Remove(mobile);
		}
	}
}


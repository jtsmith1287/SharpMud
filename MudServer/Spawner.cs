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
		[NonSerialized]
		List<Mobile> m_DeadSpawn = new List<Mobile>();
		int MaxNumberOfSpawn = 3;
		bool m_Spawning = false;
		Coordinate3 m_Location;
		[NonSerialized]
		Random m_Rand;
		[NonSerialized]
		DateTime m_SpawnTime;

		public Spawner(Room room, List<SpawnData> spawnList) {

			m_SpawnData = spawnList.ToArray();
			room.SpawnersHere.Add(this);
			m_Location = room.Location;
			ID = Guid.NewGuid();
			m_Spawning = true;

			World.Spawners.Add(this);
			World.StartAIThread();

			m_Spawns = new List<Mobile>();
			m_DeadSpawn = new List<Mobile>();
			m_Rand = new Random();
			m_SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context) {

			m_Spawning = true;
			m_Spawns = new List<Mobile>();
			m_DeadSpawn = new List<Mobile>();
			m_Rand = new Random();
			m_SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));

			World.Spawners.Add(this);
			World.StartAIThread();
		}

		public void Update() {

			if (!m_Spawning) return;

			if (m_Spawns.Count < MaxNumberOfSpawn) {
				if (m_SpawnTime < DateTime.Now) {
					Mobile newMob = new Mobile(m_SpawnData[m_Rand.Next(0, m_SpawnData.Length)], this);
					m_Spawns.Add(newMob);
					newMob.Stats.OnZeroHealth += QueueDestroyMob;
					newMob.Move(m_Location);
					World.Mobiles.Add(newMob.ID, newMob);

					m_SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));
				}
			}
			foreach (Mobile mob in m_Spawns) {
				if (!mob.IsDead)
					mob.ExecuteLogic();
			}

			if (m_DeadSpawn.Count > 0) {
				lock (m_DeadSpawn) {
					for (int i = 0; i < m_DeadSpawn.Count; i++) {
						DestroyMob(m_DeadSpawn[i]);
					}
					m_DeadSpawn.Clear();
				}
			}
		}

		private void DestroyMob(Mobile mob) {

			lock (m_Spawns) {
				m_Spawns.Remove(mob);
			}
			Room room = World.GetRoom(mob.Stats.Location);
			if (room != null) {
				lock (room.EntitiesHere) {
					room.EntitiesHere.Remove(mob.ID);
				}
			}
			lock (World.Mobiles) {
				World.Mobiles.Remove(mob.ID);
			}
			mob.BroadcastLocal(mob.Name + " has died!", Color.Yellow);
			mob.Stats.Location = null;
		}

		private void QueueDestroyMob(Data data) {

			foreach (Mobile spawn in m_Spawns) {
				if (spawn.ID == data.ID) {
					spawn.IsDead = true;
					spawn.Target = null;
					spawn.InCombat = false;
					m_DeadSpawn.Add(spawn);
					break;
				}
			}
		}
	}
}


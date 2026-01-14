using System.Linq;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace GameCore.Util {
	public class Spawner {

		public Guid ID;
		public SpawnData[] m_SpawnData;
		[ScriptIgnore]
		public List<Mobile> m_Spawns = new List<Mobile>();
		[ScriptIgnore]
		public List<Mobile> m_DeadSpawn = new List<Mobile>();
		public int MaxNumberOfSpawn = 1;
		public bool m_Spawning = false;
		public Coordinate3 m_Location;
		[ScriptIgnore]
		public Random m_Rand;
		[ScriptIgnore]
		public DateTime m_SpawnTime;

		public Spawner() {
			m_Spawning = true;
			m_Spawns = new List<Mobile>();
			m_DeadSpawn = new List<Mobile>();
			m_Rand = new Random();
			m_SpawnTime = DateTime.Now.Add(new TimeSpan(0, 0, 1));

			World.Spawners.Add(this);
			World.StartAIThread();
		}

		public Spawner(Room room, List<SpawnData> spawnList) : this() {

			m_SpawnData = spawnList.ToArray();
			room.SpawnersHere.Add(this);
			m_Location = room.Location;
			ID = Guid.NewGuid();
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
			int currentTick = World.CombatTick;
			foreach (Mobile mob in m_Spawns.ToArray()) {
				if (!mob.IsDead)
					mob.ExecuteLogic(currentTick);
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


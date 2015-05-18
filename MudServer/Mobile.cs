using System;
using GameCore.Util;

namespace GameCore {
	public class Mobile : BaseMobile {

		public Spawner SpawnerParent;
		public bool RoomEmpty = true;
		Random Rnd = new Random();
		// an average rate the mob will wander. Greater is less often.
		int WanderFrequency = 300;

		public Mobile(SpawnData data, Spawner spawner) {

			Name = data.Name;
			Stats = (SpawnData)data.ShallowCopy();
			SpawnerParent = spawner;
			GenerateID();
		}

		public void ExecuteLogic() {

			bool actionPerformed = false;

			if (InCombat) {
				if (RoundTimer.ElapsedMilliseconds > (long)3000) {
					Attack(Target);
					RoundTimer.Reset();
					RoundTimer.Start();
				}
			} else {
				actionPerformed = NonCombatAction();
			}


		}

		public void Attack(BaseMobile Target) {

			int dmg = Rnd.Next((int)(Stats.Str / 4f), (int)(Stats.Str / 3f));
			int dodgeVal = Rnd.Next(1, 101);
			if (dodgeVal >= 105 - Target.Stats.Dex) {
				BroadcastLocal(string.Format(
					"{0} dodged {1}'s attack!", Target.Name, Name), Color.RedD, Target.ID);
				Target.SendToClient(string.Format(
					"You dodged {0}'s attack!", Name), Color.RedD);
				SendToClient(string.Format(
					"{0} dodged your attack!", Target.Name), Color.RedD);
			} else if (false) {
				// other possibilities for no damage TBD
			} else {
				Target.Stats.Health -= dmg;
				BroadcastLocal(string.Format(
					"{0} was struck by {1} for {2} damage!", Target.Name, Name, dmg), Color.Red, Target.ID);
				Target.SendToClient(string.Format(
					"You were struck by {0} for {1} damage!", Name, dmg), Color.Red);
				SendToClient(string.Format(
					"You struck {0} for {1} damage!", Target.Name, dmg), Color.Red);
			}
		}

		bool NonCombatAction() {

			bool actionPerformed = false;
			SpawnData stats = (SpawnData)Stats;

			if (stats.Behaviour == Disposition.Hostile)
				SeekTarget();

			if (Rnd.Next(1, WanderFrequency) == 1) {
				Wander();
				return true;
			}

			return actionPerformed;
		}

		void SeekTarget() {

			PlayerEntity player;
			Mobile mob;
			foreach (Guid id in World.GetRoom(Stats.Location).EntitiesHere) {
				if (PlayerEntity.Players.TryGetValue(id, out player)) {
					Target = player;
					player.SendToClient(Name + " sees you!", Color.Red);
					break;
				} else if (World.Mobiles.TryGetValue(id, out mob)) {
					Target = mob;
					break;
				}
			}

			if (Target != null) {
				InCombat = true;
				Attack(Target);
				RoundTimer.Start();
			}
		}

		void Wander() {

			Coordinate3 vector = Coordinate3.Zero;
			int[] posNegOptions = new int[] { -1, 1 };
			int posNegChoice = posNegOptions[Rnd.Next(0, 2)];
			int x_y_or_z = Rnd.Next(1, 4);

			switch (x_y_or_z) {
				case 1:
					vector.X += posNegChoice;
					break;
				case 2:
					vector.Y += posNegChoice;
					break;
				case 3:
					vector.Z += posNegChoice;
					break;
			}
			//TODO: Try to only attempt to move in known existing directions.
			if (World.GetRoom(Stats.Location + vector) != null) {
				Move(Stats.Location + vector);
			}

		}

		void OnDeath() {

			SpawnerParent.Remove(this);
			World.Mobiles.Remove(ID);
		}
	}
}


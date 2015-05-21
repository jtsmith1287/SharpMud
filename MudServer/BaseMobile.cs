using System;
using System.Collections.Generic;
using System.Diagnostics;
using GameCore.Util;

namespace GameCore {
	public class BaseMobile {

		public Guid ID;
		public string Name;
		public Data Stats;
		public bool InCombat = false;
		public bool IsDead = false;
		public bool Hidden = false;
		protected Random Rnd = new Random();
		public BaseMobile Target;
		protected Stopwatch RoundTimer = new Stopwatch();
		/// <summary>
		/// Do not add to this event directly. Use OnDeath.
		/// </summary>
		event Action<BaseMobile> OnDeathEvent = delegate { };
		public event Action<BaseMobile> OnDeath {
			add {
				//Prevent double subscription
				OnDeathEvent -= value;
				OnDeathEvent += value;
			}
			remove {
				OnDeathEvent -= value;
			}
		}

		/// <summary>
		/// Generates and sets this instance's Guid.
		/// </summary>
		/// <returns>The generated Guid.</returns>
		public Guid GenerateID() {

			ID = Guid.NewGuid();
			Stats.ID = ID;
			return ID;
		}

		public void TriggerOnDeath(Data data) {

			if (data.ID == Stats.ID) {
				OnDeathEvent(this);
			}
		}

		public virtual void Move(Coordinate3 location) {

			Room oldRoom = World.GetRoom(Stats.Location);
			Room newRoom = World.GetRoom(location);

			if (Stats.Location == location) {
				BroadcastLocal(Name + " has arrived.", Color.Yellow);
				lock (newRoom.EntitiesHere) {
					try {
						// In case we're already here, we don't want to add a duplicate of ourselves.
						newRoom.EntitiesHere.Remove(ID);
					} catch (InvalidOperationException) {

					}
					newRoom.EntitiesHere.Add(ID);
				}
				var thisPlayer = PlayerEntity.GetPlayerByID(Stats.ID);
				if (thisPlayer != null)
					Actions.ActionCalls["look"](thisPlayer, new string[1]);
				return;
			}

			if (oldRoom != null) {
				oldRoom.EntitiesHere.Remove(ID);
				if (!Hidden) {
					BroadcastLocal(Name + " has left to the " + oldRoom.GetDirection(newRoom.Location),
											Color.Yellow);
				}
			}
			if (newRoom != null) {
				Stats.Location = newRoom.Location;
				if (oldRoom != null) {
					if (!Hidden) {
						BroadcastLocal(
							Name + " has arrived from the " +
							newRoom.GetDirection(oldRoom.Location), Color.Yellow);
						SendToClient("You move to the " + oldRoom.GetDirection(newRoom.Location),
							Color.White);
					} else {
						SendToClient("You sneak to the " + oldRoom.GetDirection(newRoom.Location),
							Color.Magenta);
					}
				} else {
					if (!Hidden) {
						BroadcastLocal(Name + " has arrived.", Color.Yellow);
					}
				}
				newRoom.EntitiesHere.Add(ID);
				var thisPlayer = PlayerEntity.GetPlayerByID(Stats.ID);
				if (thisPlayer != null)
					Actions.ActionCalls["look"](thisPlayer, new string[1]);
			}
		}

		public virtual void SendToClient(string msg, string colorSequence = "") {

			PlayerEntity player;
			if (PlayerEntity.Players.TryGetValue(ID, out player)) {
				player.SendToClient(msg, colorSequence);
			}
		}

		public void BroadcastLocal(string msg, string colorSequence = "", params Guid[] ignore) {

			PlayerEntity player;
			Room room = World.GetRoom(Stats.Location);
			if (room != null) {
				for (int i = 0; i < room.EntitiesHere.Count; i++) {
					if (PlayerEntity.Players.TryGetValue(room.EntitiesHere[i], out player)) {
						if (player.ID == ID || Contains<Guid>(ignore, player.ID)) {
							continue;
						}
						player.SendToClient(msg, colorSequence);
					}
				}
			}
		}

		protected bool Contains<T>(T[] a, T b) {

			for (int i = 0; i < a.Length; i++) {
				if (a[i].Equals(b))
					return true;
			}
			return false;
		}

		protected void StrikeTarget(BaseMobile target) {

			int dodgeVal = Rnd.Next(1, 101);
			if (dodgeVal >= 108 - Math.Sqrt((double)target.Stats.Dex * 7)) {
				BroadcastLocal(string.Format(
					"{0} dodged {1}'s attack!", target.Name, Name), Color.RedD, Target.ID);
				target.SendToClient(string.Format(
					"You dodged {0}'s attack!", Name), Color.RedD);
				SendToClient(string.Format(
					"{0} dodged your attack!", target.Name), Color.RedD);
			} else if (false) {
				// other possibilities for no damage TBD
			} else if (target.Stats.Health > 0 && !target.IsDead) {
				int dmg = Rnd.Next((int)(Stats.Str / 4f), (int)(Stats.Str / 3f));
				if (Hidden) {
					Hidden = false;
					SendToClient("You surprise attack " + target.Name + "!", Color.White);
					dmg *= 2;
				}
				target.OnDeath += OnDeathEventReceiver;
				BroadcastLocal(string.Format(
					"{0} was struck by {1} for {2} damage!", target.Name, Name, dmg), Color.Red, target.ID);
				target.SendToClient(string.Format(
					Color.Yellow + "*" +
					Color.White + "You " + Color.Red +
					"were struck by {0} for {1} damage!", Name, dmg));
				SendToClient(string.Format(
					Color.White + "You " + Color.Red +
					"struck {0} for {1} damage!", target.Name, dmg), Color.Red);
				target.DisplayVitals();
				target.Stats.Health -= dmg;
			} else {
				Target = null;
				InCombat = false;
				SendToClient("* Combat disengaged *", Color.White);
			}
		}

		void OnDeathEventReceiver(BaseMobile killed) {

			// If this mobile is dead, it can't get experience now can it?
			if (IsDead) { return; }
			int distance = (Stats.Location - killed.Stats.Location).Max();
			// If targetting something far away and it dies, ignore it.
			if (distance > 3 && Target.Stats.ID == killed.Stats.ID) {
				InCombat = false;
				Target = null;
				return;
			}
			// Only untarget and break combat if we're targetting what died and grant bonus experience.
			if (Target.Stats.ID == killed.Stats.ID) {
				SendToClient(
					"You killed " + killed.Name + "! You've gained 5 bonus experience.", Color.Cyan);
				Stats.Exp += 5;
				InCombat = false;
				Target = null;
			}
			int exp = killed.Stats.GrantExperience();
			SendToClient("You've gained " + exp + " experience!", Color.Cyan);
			Stats.Exp += exp;
			// Target is dead, so unsubscribe to prevent any weird chain event triggers.
			killed.OnDeath -= OnDeathEventReceiver;
			SendToClient("* Combat disengaged *", Color.White);
		}

		public void DisplayVitals() {

			int barWidth = 20;
			string vitalsColor = Color.Green;
			string enemyHealth = "";
			int barCount;
			string bars;
			string spaces;

			// A yellow '+' to indicate the player can allocate a new stat point.
			string levelUpIndicator = Stats.StatAllocationNeeded ? Color.Yellow + " + " : "";
			string enemyHealthColor = Color.Green;
			if ((float)Stats.Health / Stats.MaxHealth < 0.33f)
				vitalsColor = Color.Red;
			if (Target != null) {
				barCount = (int)(barWidth * (float)Target.Stats.Health / Target.Stats.MaxHealth);
				bars = new String('#', barCount);
				spaces = new String(' ', 20 - barCount);
				enemyHealth = string.Format("{0}: {1}<-[{2}{3}]->{4}",
					Target.Name,
					Target.Stats.Health,
					bars,
					spaces,
					Stats.MaxHealth);
				if ((float)Target.Stats.Health / Target.Stats.MaxHealth < 0.33f)
					enemyHealthColor = Color.Red;
			}

			barCount = (int)(barWidth * (float)Stats.Health / Stats.MaxHealth);
			bars = new String('#', barCount);
			spaces = new String(' ', 20 - barCount);
			string healthBar = string.Format("{0}<-[{1}{2}]->{3}",
				Stats.Health,
				bars,
				spaces,
				Stats.MaxHealth);

			SendToClient(string.Format("-- HP: {0} -- {1}{2}{3}",
				healthBar,
				enemyHealthColor,
				enemyHealth,
				levelUpIndicator), vitalsColor);
		}
	}
}


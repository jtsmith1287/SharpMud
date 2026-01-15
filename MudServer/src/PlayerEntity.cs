using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ServerCore;
using GameCore.Util;
using System.Runtime.Serialization;
using System.Threading;

namespace GameCore {
	public class PlayerEntity : BaseMobile {

		public static Dictionary<Guid, PlayerEntity> Players = new Dictionary<Guid, PlayerEntity>();
		public static Thread PlayerThread;
		System.Diagnostics.Stopwatch HealTick = new System.Diagnostics.Stopwatch();
		System.Diagnostics.Stopwatch ReviveTick = new System.Diagnostics.Stopwatch();
		int TickDuration = 3000;
		Connection Conn;

		public Coordinate3 Location {
			get {
				return Stats.Location;
			}
			set {
				Stats.Location = value;
			}
		}

		public bool GodMode { get; set; }

		public bool Admin = true;
		public PlayerState State;

		public static PlayerEntity GetPlayerByID(Guid id) {

			PlayerEntity player;
			Players.TryGetValue(id, out player);
			return player;
		}

		public PlayerEntity(Connection conn, Data data) {

			Conn = conn;
			ID = data.Id;
			Name = data.Name;
			Stats = data;
			Stats.OnZeroHealth += Die;
			Stats.ThisBaseMobile = this;
			Stats.OnZeroHealth += Die;
			Players.Add(ID, this);


			// The player hasn't been initialized yet.
			if (Stats.MaxHealth == 0) {
				Stats.MaxHealth = 15;
				Stats.Health = Stats.MaxHealth;
			}

			if (data.Location != null && World.GetRoom(data.Location) != null) {
				Move(data.Location);
				if (data.Location == Coordinate3.Purgatory) {
					IsDead = true;
					ReviveTick.Start();
				}
			} else {
				Move(Coordinate3.Zero);
			}

			Conn.Send("Welcome!");
			State = PlayerState.Active;
			DisplayVitals();
			HealTick.Start();

			if (PlayerThread == null) {
				PlayerThread = new Thread(RunAllPlayerLogic);
				PlayerThread.Start();
				Console.WriteLine("PlayerThread started.");
			}
		}

		/// <summary>
		/// Runs all persistent logic for all players on a separate thread.
		/// </summary>
		void RunAllPlayerLogic() {

			try {
				while (true) {
					int currentTick = World.CombatTick;
					lock (Players) {
						foreach (var entry in Players.ToArray()) {
							if (entry.Value.State == PlayerState.Active || entry.Value.State == PlayerState.Resting) {
								entry.Value.ExecuteLogic(currentTick);
							}
						}
					}
					Thread.Sleep(33);
				}
			} catch (ThreadAbortException) {

				Console.WriteLine("PlayerThread aborted.");
			}
		}

		void ExecuteLogic(int currentTick) {

			if (IsDead) {
				if (ReviveTick.ElapsedMilliseconds > 5000) {
					Stats.Health = Stats.MaxHealth;
					IsDead = false;
					Move(Coordinate3.Zero);
					SendToClient("You have been revived! Welcome back to the land of the living.", Color.Green);
					ReviveTick.Reset();
				}
				return;
			}

			if (InCombat && Target != null) {
				if (State == PlayerState.Resting) {
					State = PlayerState.Active;
					SendToClient("You jump up as combat begins!", Color.Yellow);
				}
				if (LastCombatTick < currentTick) {
					int ticksPassed = currentTick - (LastCombatTick == -1 ? currentTick : LastCombatTick);
					if (LastCombatTick == -1) ticksPassed = 1; // Handle first tick

					float speedModifier = 1.0f + (Stats.GetTickModifier() / 3000f);
					CombatEnergy += ticksPassed * speedModifier;

					while (CombatEnergy >= 1.0f) {
						if (Target == null || Target.Stats == null || Target.IsDead || Target.Stats.Health <= 0 || Target.Stats.Location != Stats.Location) {
							InCombat = false;
							Target = null;
							CombatEnergy = 0;
							SendToClient("*Target lost. Combat disengaged!*", Color.White);
							break;
						} else {
							StrikeTarget(Target);
							CombatEnergy -= 1.0f;
						}
					}
					DisplayVitals();
					LastCombatTick = currentTick;
				}
			} else if (!InCombat) {
				CombatEnergy = 0;
				if (HealTick.ElapsedMilliseconds > TickDuration) {
					if (State == PlayerState.Resting) {
						int healAmount = (int)(Stats.MaxHealth * 0.10f);
						if (healAmount < 1) healAmount = 1;
						Stats.Health += healAmount;
						SendToClient(string.Format("You feel better... (+{0} HP)", healAmount), Color.Green);

						if (Stats.Health >= Stats.MaxHealth) {
							Stats.Health = Stats.MaxHealth;
							State = PlayerState.Active;
							SendToClient("You are fully rested and stand up.", Color.Cyan);
						}
					} else {
						Stats.Health += Rnd.Next((int)(Stats.Int / 4f), (int)(Stats.Int / 3f));
					}
					HealTick.Restart();
				}
			}
		}

		public void Close() {

			lock (Players) {
				Players.Remove(ID);
			}
			Target = null;
			InCombat = false;
		}

		public override void SendToClient(string msg, string colorSequence = "") {

			//TODO: Word wrap this to 80 characters
			Conn.Send(colorSequence + msg + Color.Reset);
		}

		public string WaitForClientReply() {

			try {
				string reply = Conn.Reader.ReadLine();
				if (reply != null) {
					return reply.Trim();
				} else {
					// null
					return reply;
				}
			} catch (IOException) {
				// Player disconnected.
				return null;
			}
		}

		private void Die(Data data) {

			IsDead = true;
			InCombat = false;
			Target = null;
			SendToClient("You have been slain!");
			Move(Coordinate3.Purgatory);
			ReviveTick.Start();
		}

		internal void OnDisconnect() {
			Conn.OnDisconnect();
		}
	}
}


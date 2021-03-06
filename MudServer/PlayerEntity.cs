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
		System.Diagnostics.Stopwatch CombatTick = new System.Diagnostics.Stopwatch();
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

		public bool Admin = true;
		public PlayerState State;

		public static PlayerEntity GetPlayerByID(Guid id) {

			PlayerEntity player;
			Players.TryGetValue(id, out player);
			return player;
		}

		public PlayerEntity(Connection conn, Data data) {

			Conn = conn;
			ID = data.ID;
			Name = data.Name;
			Stats = data;
			Stats.OnZeroHealth += Die;
			Stats.thisBaseMobile = this;
			Stats.OnZeroHealth += Die;
			Players.Add(ID, this);


			// The player hasn't been initialized yet.
			if (Stats.MaxHealth == 0) {
				Stats.MaxHealth = 15;
				Stats.Health = Stats.MaxHealth;
			}

			if (data.Location != null) {
				Move(data.Location);
			} else {
				Move(Coordinate3.Zero);
			}

			Conn.Send("Welcome!");
			State = PlayerState.Active;
			DisplayVitals();
			CombatTick.Start();
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
					lock (Players) {
						foreach (var entry in Players) {
							if (entry.Value.State == PlayerState.Active) {
								entry.Value.ExecuteLogic();
							}
						}
					}
					Thread.Sleep(33);
				}
			} catch (ThreadAbortException) {

				Console.WriteLine("PlayerThread aborted.");
			}
		}

		void ExecuteLogic() {

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
				if (CombatTick.ElapsedMilliseconds >= TickDuration - Stats.GetTickModifier()) {
					if (Target.Stats.Location != Stats.Location) {
						InCombat = false;
						Target = null;
						SendToClient("*Target lost. Combat disengaged!*", Color.White);
					} else {
						StrikeTarget(Target);
						CombatTick.Restart();
					}
					DisplayVitals();
				}
			} else if (!InCombat) {
				if (HealTick.ElapsedMilliseconds > TickDuration) {
					Stats.Health += Rnd.Next((int)(Stats.Int / 4f), (int)(Stats.Int / 3f));
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
			Move(new Coordinate3(int.MaxValue, int.MaxValue, int.MaxValue));
			ReviveTick.Start();
		}

		internal void OnDisconnect() {
			Conn.OnDisconnect();
		}
	}
}


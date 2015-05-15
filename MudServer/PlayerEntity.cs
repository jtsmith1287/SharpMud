using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ServerCore;
using GameCore.Util;

namespace GameCore {
	public class PlayerEntity : BaseMobile {

		public static Dictionary<Guid, PlayerEntity> Players = new Dictionary<Guid, PlayerEntity>();
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
			Players.Add(ID, this);

			if (new Coordinate3(1, 2, 3) == new Coordinate3(1, 2, 3)) {
				Console.WriteLine("YAAAYYY!!");
			}

			// The player hasn't been initialized yet.
			if (Stats.MaxHealth == 0) {
				Stats.MaxHealth = 15;
				Stats.Health = Stats.MaxHealth;
			}

			if (data.Location != null) {
				Console.WriteLine("Moving to existing position");
				Move(data.Location);
			} else {
				Console.WriteLine("Moving to 0,0,0");
				Move(Coordinate3.Zero);
			}

			Conn.Send("Welcome!");
			State = PlayerState.Active;
		}

		public void Close() {

			Conn.SendClosingMessage();
			if (Conn.socket.Connected) {
				Conn.socket.Close();
			}
			Players.Remove(ID);
			Conn.Dispose();
		}

		public override void SendToClient(string msg) {

			Conn.Send(msg);
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
	}
}


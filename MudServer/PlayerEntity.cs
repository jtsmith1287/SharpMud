using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ServerCore;
using GameCore.Util;
using System.Runtime.Serialization;

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
		}

		public void Close() {

			Conn.SendClosingMessage();
			if (Conn.socket.Connected) {
				Conn.socket.Close();
			}
			Players.Remove(ID);
			Conn.Dispose();
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

			SendToClient("You have been slain!");
			Move(Coordinate3.Zero);
			data.Health = data.MaxHealth;
		}

		[OnDeserialized]
		void OnDeserialized() {
			Stats.OnZeroHealth += Die;
		}
	}
}


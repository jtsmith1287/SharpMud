using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using GameCore;
using ServerCore.Util;

namespace ServerCore {
	public class Connection {

		static object BigLock = new object ();
		Socket socket;
		public StreamReader Reader;
		public StreamWriter Writer;
		static ArrayList connections = new ArrayList ();
		public PlayerEntity Player;

		public Connection (Socket socket) {

			this.socket = socket;
			Reader = new StreamReader (new NetworkStream (socket, false));
			Writer = new StreamWriter (new NetworkStream (socket, true));
			new Thread (ClientLoop).Start ();
		}

		void ClientLoop () {
	
			try {
				lock (BigLock) {
					OnConnect ();
				}
				while (true) {
					lock (BigLock) {
						foreach (Connection conn in connections) {
							conn.Writer.Flush ();
						}
					}
					string line = Reader.ReadLine ();
					if (line == null) {
						break;
					}
					lock (BigLock) {
						ArgumentHandler.HandleLine (line, Player);
					}
				}
			} finally {
				lock (BigLock) {
					SendClosingMessage ();
					socket.Close ();
					OnDisconnect ();
				}
			}
		}

		public void Send (string msg) {
			
			Writer.WriteLine (msg);
		}

		void SendClosingMessage () {
		
			Send ("Thanks for playing! You're being cleanly disconnected now. Bye!");
		}

		void OnConnect () {
			
			bool userFound = false;
			string response;
			
			while (!userFound) {
				Send ("Please log in.\nWhat is your username? (If we don't locate your account we'll create a new one):");
				response = Reader.ReadLine ();
				Console.WriteLine (response);
				
			}
			
			Player.State = GameCore.Util.PlayerState.LoggingIn;
			connections.Add (this);
		}

		void OnDisconnect () {
		
			connections.Remove (this);
			Player.Close ();
			Console.WriteLine (Player.ID + " has disconnected");
		}
	}
}
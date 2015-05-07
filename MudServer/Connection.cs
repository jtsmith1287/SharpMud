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
			Writer = new StreamWriter(new NetworkStream(socket, true));
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
						ArgumentHandler.HandleLine (line.Trim(), Player);
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
			Writer.Flush();
		}

		void SendClosingMessage () {
		
			Send ("Thanks for playing! You're being cleanly disconnected now. Bye!");
		}

		void OnConnect () {

			string username;
			string providedPwd;
			string truePwd;
			
			// Get user login information and create a new account if one doesn't exist.
			while (true) {
				Send ("Please log in.\nIf we don't locate your account we'll create a new one\nUsername:\n");
				// Wait for user to input username
				username = Reader.ReadLine ().Trim();
				if (Data.UsernamePwdPairs.TryGetValue(username, out truePwd)) {
					Send("Password:\n");
					// Wait for user to input password
					providedPwd = Reader.ReadLine();
					if (providedPwd == truePwd) {
						Player = new PlayerEntity(this, Data.UsernameIDPairs[username], username);
						break;
					} else {
						Send("Incorrect password. Sorry.");
						this.socket.Close();
						return;
					}
				// Create a new user account because we didn't find one.
				} else {
					string pwdVerify;
					Send("You must be new. We've got your name now, so how about a password?\nType carefully.");
					while (true){
						providedPwd = Reader.ReadLine();
						Send("Verify that please...\n:");
						pwdVerify = Reader.ReadLine();
						if (providedPwd == pwdVerify) {
							Data.UsernamePwdPairs.Add(username, providedPwd);
							Send("Got it! We're entering you into the system now.");
							Player = new PlayerEntity(this, Guid.NewGuid(), username);
							Data.UsernameIDPairs.Add(username, Player.ID);
							Data.IDDataPairs.Add(Player.ID, Player.Stats);
							Send("Alright, " + username + ". You're good to go!");
							
							break;
						} else {
							Send("I uh... Hmm. These don't match. Let's try that again. Be a little more careful.");
							continue;
						}
					}
					// At this point we've either loaded, or created a new user, so exit the loop.
					break;
				}
			}
			connections.Add (this);
			Console.WriteLine("Starting save thread for new data");
			new Thread(Data.SaveStaticData).Start();
		}

		void OnDisconnect () {
		
			connections.Remove (this);
			Player.Close ();
			Console.WriteLine (Player.ID + " has disconnected");
		}
	}
}

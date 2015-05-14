using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameCore;
using GameCore.Util;

namespace ServerCore {
	class Server {

		const int PortNumber = 4000;
		const int BacklogSize = 20;

		internal void Run () {

			Console.CancelKeyPress += SaveData;

			LoadData ();
			BuildWorld ();

			Socket server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind (new IPEndPoint (IPAddress.Any, PortNumber));
			server.Listen (BacklogSize);

			Console.WriteLine ("Server booted. Listening for connections.");
			while (true) {
				Socket conn = server.Accept ();
				Connection conObject = new Connection (conn);
				Console.WriteLine ("Connection accepted -- " + conn.RemoteEndPoint);
			}
		}

		private void SaveData (object sender, ConsoleCancelEventArgs e) {

			Console.WriteLine ("SAVING");
			World.StopAllSpawnThreads ();
			Data.SaveStaticData ();
		}

		private void LoadData () {

			Data.LoadStaticData ();
		}

		void BuildWorld () {

			new Room (Coordinate3.Zero, "Starting Room");
		}
	}
}
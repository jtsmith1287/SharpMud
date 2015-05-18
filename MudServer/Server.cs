using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using GameCore;
using GameCore.Util;

namespace ServerCore {
	class Server {

		const int PortNumber = 4000;
		const int BacklogSize = 20;

		internal void Run() {

			Console.CancelKeyPress += CleanServerShutdown;

			LoadData();
			BuildWorld();

			Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
			server.Listen(BacklogSize);
			Console.WriteLine("Server booted. Listening for connections.");

			Thread thread = new Thread(() => Application.Run(new MobMaker()));
			thread.Start();
			Console.WriteLine("Opening Mob Maker.");

			while (true) {
				Socket conn = server.Accept();
				Connection conObject = new Connection(conn);
				Console.WriteLine("Connection accepted -- " + conn.RemoteEndPoint);
			}
		}

		private void CleanServerShutdown(object sender, ConsoleCancelEventArgs e) {

			Console.WriteLine("Aborting threads...");
			World.StopAllSpawnThreads();
			Console.WriteLine("Terminating connections and disposing of resources...");
			PlayerEntity[] players = new PlayerEntity[PlayerEntity.Players.Count];
			PlayerEntity.Players.Values.CopyTo(players, 0);
			foreach (var player in players) {
				player.Close();
			}
			Console.WriteLine("Saving data...");
			Data.SaveData();
			Console.Write("Press ENTER to close the console.");
			Console.ReadLine();
		}

		private void LoadData() {

			Data.LoadData();
			//Data.PopulateSpawnDataTemplates ();
		}

		void BuildWorld() {

			new Room(Coordinate3.Zero, "Starting Room");
		}
	}
}
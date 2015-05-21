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

			Data.LoadData();
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

			Console.WriteLine("Terminating connections and disposing of resources...");
			if (PlayerEntity.PlayerThread != null) {
				PlayerEntity.PlayerThread.Abort();
			}
			PlayerEntity[] players = new PlayerEntity[PlayerEntity.Players.Count];
			PlayerEntity.Players.Values.CopyTo(players, 0);
			foreach (var player in players) {
				player.SendToClient("Thanks for playing! The server is shutting down now!");
				player.OnDisconnect();
			}
			Console.WriteLine("Aborting threads...");
			World.StopAllSpawnThreads();
			Console.WriteLine("Saving data...");
			Data.SaveData(
				DataPaths.IdData,
				DataPaths.Spawn,
				DataPaths.UserId,
				DataPaths.UserPwd,
				DataPaths.World);
			Console.Write("Press ENTER to close the console.");
			Console.ReadLine();
		}

		void BuildWorld() {

			new Room(Coordinate3.Zero, "Starting Room");
			new Room(new Coordinate3(int.MaxValue, int.MaxValue, int.MaxValue), "Purgatory");
		}
	}
}
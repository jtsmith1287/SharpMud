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
    const int PortNumber = 4321;
    const int BacklogSize = 20;

    private string _ipAddress;

    internal void Run() {
        Console.CancelKeyPress += CleanServerShutdown;

        DataManager.LoadData();
        BuildWorld();

        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
        server.Listen(BacklogSize);

        try {
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            Console.WriteLine("Local IP Addresses:");
            foreach (IPAddress ip in hostEntry.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    _ipAddress = ip.ToString();
                }
            }
        } catch (Exception) {
            Console.WriteLine("Could not determine local IP addresses.");
        }

        Console.WriteLine("Server booted on " + _ipAddress + ". Listening for connections on port " + PortNumber);

     			Thread thread = new Thread(() => Application.Run(new ContentCreator()));
				ContentCreator.OnMapSaved += (s, e) => {
					// Reload maps
					DataManager.LoadData(DataPaths.World);
					// Check players
					foreach (var player in PlayerEntity.Players.Values) {
						string coordKey = string.Format("{0} {1} {2}", player.Stats.Location.X, player.Stats.Location.Y, player.Stats.Location.Z);
						if (!World.Rooms.ContainsKey(coordKey)) {
							player.Stats.Location = new Coordinate3(0, 0, 0);
							player.SendToClient("The room you were in has been deleted. Moving you to the starting room.", Color.Yellow);
						}
					}
				};
        thread.Start();
        Console.WriteLine("Opening Mob Maker on new thread.");

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
        DataManager.SaveData(
            DataPaths.IdData,
            DataPaths.Creatures,
            DataPaths.UserId,
            DataPaths.UserPwd
        );
        Console.Write("Press ENTER to close the console.");
        Console.ReadLine();
    }

    void BuildWorld() {
        // Rooms are now loaded from maps.json (and fallback to world.json if maps.json is missing).
        // Building the world from code is disabled to prefer data-driven world loading.
        /*
        new Room(Coordinate3.Zero, "Starting Room");
        new Room(Coordinate3.Purgatory, "Purgatory");
        */
    }
}
}

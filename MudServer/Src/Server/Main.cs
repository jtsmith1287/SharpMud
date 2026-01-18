using System;
using MudServer.Entity;
using MudServer.World;
using MudServer.Enums;

namespace MudServer.Server {
	public class Program {
		public static void Main(string[] args) {
			if (args.Length > 0 && args[0] == "--test") {
				new MudServer.Entity.CombatTests().RunTests();
				new MudServer.Entity.SpawnerTests().RunTests();
				new MudServer.World.RoomTests().RunTests();
				new MudServer.Actions.ActionTests().RunTests();
				Environment.Exit(0);
			}
			try {
				Server server = new Server();
				server.Run();
			} catch (Exception e) {
				Console.WriteLine("Woah woah, something happened! Saving data but it may be corrupt.");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.GetType().ToString());
				Console.WriteLine(e.StackTrace);
			} finally {
				DataManager.SaveAllMaps();
				DataManager.SaveData(
					DataPaths.IdData,
					DataPaths.Creatures,
					DataPaths.UserId,
					DataPaths.UserPwd);
				Console.ReadLine();
			}
		}
	}
}


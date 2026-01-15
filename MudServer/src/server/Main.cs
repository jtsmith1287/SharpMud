using System;
using GameCore;
using GameCore.Util;

namespace ServerCore {
	public class Program {
		public static void Main(string[] args) {
			if (args.Length > 0 && args[0] == "--test") {
				new MudServer.CombatTests().RunTests();
				new MudServer.SpawnerTests().RunTests();
				return;
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
				Data.SaveAllMaps();
				Data.SaveData(
					DataPaths.IdData,
					DataPaths.Creatures,
					DataPaths.UserId,
					DataPaths.UserPwd);
				Console.ReadLine();
			}
		}
	}
}


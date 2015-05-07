using System;
using System.Collections.Generic;

namespace GameCore {
	public class Data {
		
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string> ();
		public static Dictionary<string, Guid> UsernameIDPairs = new Dictionary<string, Guid> ();
		public static Dictionary<Guid, Data> IDDataPairs = new Dictionary<Guid, Data> ();
		#region Exposed Data
		public string Name;
		public int Level;
		#endregion
		public static Data GetData (Guid id) {
			
			Data data;
			if (IDDataPairs.TryGetValue (id, out data)) {
				return data;
			} else {
				return new Data (PlayerEntity.Players [id].Name);
			}
		}

		public Data (string name) {
			
			Name = name;
			Level = 1;
		}
	}
}


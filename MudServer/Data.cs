using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using GameCore;
using GameCore.Util;

namespace GameCore {
	[Serializable]
	public class Data {
		
		[NonSerialized]
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string> ();
		[NonSerialized]
		public static Dictionary<string, Guid> UsernameIDPairs = new Dictionary<string, Guid> ();
		[NonSerialized]
		public static Dictionary<Guid, Data> IDDataPairs = new Dictionary<Guid, Data> ();

		#region Exposed Data
		public string Name;
		public int Level;
		public Coordinate3 Location;
		#endregion

		public static Data GetData (Guid id) {
			
			Data data;
			if (IDDataPairs.TryGetValue (id, out data)) {
				return data;
			} else {
				return data;
			}
		}

		public Data (string name) {
			
			Name = name;
			Level = 1;
		}

		internal static void SaveStaticData() {

			BinaryFormatter bf = new BinaryFormatter();
			Stream stream = new FileStream("C:\\MudServer\\userpwd", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, UsernamePwdPairs);
			stream.Close();

			stream = new FileStream("C:\\MudServer\\userid", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, UsernameIDPairs);
			stream.Close();

			stream = new FileStream("C:\\MudServer\\iddata", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, IDDataPairs);
			stream.Close();
		}

		internal static void LoadStaticData() {

			try {
				BinaryFormatter bf = new BinaryFormatter();
				Stream stream = new FileStream("C:\\MudServer\\userpwd", FileMode.Open, FileAccess.Read, FileShare.Read);
				UsernamePwdPairs = (Dictionary<string, string>)bf.Deserialize(stream);
				stream.Close();

				stream = new FileStream("C:\\MudServer\\userid", FileMode.Open, FileAccess.Read, FileShare.Read);
				UsernameIDPairs = (Dictionary<string, Guid>)bf.Deserialize(stream);
				stream.Close();

				stream = new FileStream("C:\\MudServer\\iddata", FileMode.Open, FileAccess.Read, FileShare.Read);
				IDDataPairs = (Dictionary<Guid, Data>)bf.Deserialize(stream);
				stream.Close();

			} catch (IOException) {
				Data.SaveStaticData();
			}
		}
	}
}


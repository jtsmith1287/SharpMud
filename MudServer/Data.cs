using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using GameCore;
using GameCore.Util;

namespace GameCore {
	[Serializable]
	public class Data {

		#region Static Fields
		[NonSerialized]
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string> ();
		[NonSerialized]
		public static Dictionary<string, Guid> UsernameIDPairs = new Dictionary<string, Guid> ();
		[NonSerialized]
		public static Dictionary<Guid, Data> IDDataPairs = new Dictionary<Guid, Data> ();
		[NonSerialized]
		public static Dictionary<string, SpawnData> NameSpawnPairs = new Dictionary<string, SpawnData> ();
		#endregion
		#region Exposed Data
		public string Name;
		public int Level;
		public Coordinate3 Location;
		public Guid ID;
		int m_Health;
		public int MaxHealth;

		public int Health {
			get {
				return m_Health;
			}
			set {
				m_Health = value;
				if (m_Health > MaxHealth)
					m_Health = MaxHealth;
				if (m_Health < 0) {
					m_Health = 0;
					OnZeroHealth ();
				}
			}
		}

		public delegate void BaseDelegate ();

		public event BaseDelegate OnZeroHealth;
		#endregion
		protected Data () {
		}

		public Data (string username, Guid id) {

			Name = username;
			ID = id;
			Level = 1;
			Data.IDDataPairs.Add (id, this);
		}

		public static Data GetData (Guid id) {

			Data data;
			if (IDDataPairs.TryGetValue (id, out data)) {
				return data;
			} else {
				return data;
			}
		}
		#region Data Saving/Loading
		internal static void SaveData () {

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream stream = new FileStream ("userpwd", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize (stream, UsernamePwdPairs);
			Console.WriteLine (string.Format ("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
			stream.Close ();

			stream = new FileStream ("userid", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize (stream, UsernameIDPairs);
			Console.WriteLine (string.Format ("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
			stream.Close ();

			stream = new FileStream ("iddata", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize (stream, IDDataPairs);
			Console.WriteLine (string.Format ("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
			stream.Close ();

			stream = new FileStream ("world", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize (stream, World.Rooms);
			Console.WriteLine (string.Format ("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
			stream.Close ();
		}

		internal static void LoadData () {

			try {
				long bytes = 0;
				BinaryFormatter bf = new BinaryFormatter ();
				Stream stream = new FileStream ("userpwd", FileMode.Open, FileAccess.Read, FileShare.Read);
				UsernamePwdPairs = (Dictionary<string, string>)bf.Deserialize (stream);
				bytes += stream.Length;
				stream.Close ();

				stream = new FileStream ("userid", FileMode.Open, FileAccess.Read, FileShare.Read);
				UsernameIDPairs = (Dictionary<string, Guid>)bf.Deserialize (stream);
				bytes += stream.Length;
				stream.Close ();

				stream = new FileStream ("iddata", FileMode.Open, FileAccess.Read, FileShare.Read);
				IDDataPairs = (Dictionary<Guid, Data>)bf.Deserialize (stream);
				bytes += stream.Length;
				stream.Close ();

				stream = new FileStream ("world", FileMode.Open, FileAccess.Read, FileShare.Read);
				World.Rooms = (Dictionary<string, Room>)bf.Deserialize (stream);
				bytes += stream.Length;
				stream.Close ();

				Console.WriteLine (string.Format ("Loaded {0}kb of data into memory.", bytes / 1000f));

			} catch (IOException) {
				Data.SaveData ();
			}
		}
		#endregion
		// Temporary data populating until tool is created to replace this.
		public static void PopulateSpawnDataTemplates () {
			
			var orc = new SpawnData ("Orc");
			Data.NameSpawnPairs.Add (orc.Name, orc);
			var goblin = new SpawnData ("Goblin");
			Data.NameSpawnPairs.Add (goblin.Name, goblin);
			var spider = new SpawnData ("Spider");
			Data.NameSpawnPairs.Add (spider.Name, spider);
		}

		public Data ShallowCopy () {
		
			return (Data)this.MemberwiseClone ();
		}
	}
}


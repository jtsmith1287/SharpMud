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
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string>();
		[NonSerialized]
		public static Dictionary<string, Guid> UsernameIDPairs = new Dictionary<string, Guid>();
		[NonSerialized]
		public static Dictionary<Guid, Data> IDDataPairs = new Dictionary<Guid, Data>();
		[NonSerialized]
		public static Dictionary<string, SpawnData> NameSpawnPairs = new Dictionary<string, SpawnData>();
		#endregion
		#region Exposed Data
		[NonSerialized]
		public BaseMobile thisBaseMobile;
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
				// If this object is dead, we can't make it more dead.
				if (m_Health == 0 && value < 0) {
					return;
				}
				m_Health = value;
				if (m_Health > MaxHealth)
					m_Health = MaxHealth;
				if (m_Health <= 0) {
					m_Health = 0;
					thisBaseMobile.Target = null;
					thisBaseMobile.InCombat = false;
					OnZeroHealthEvent(this);
					try {
						thisBaseMobile.TriggerOnDeath(this);
					} catch (NullReferenceException) {

					}
				}
			}
		}
		int m_Str = 10;
		public int BonusStr;
		public int Str {
			get {
				return m_Str + BonusStr;
			}
			set {
				m_Str = value;
			}
		}
		int m_Dex = 10;
		public int BonusDex;
		public int Dex {
			get {
				return m_Dex + BonusDex;
			}
			set {
				m_Dex = value;
			}
		}
		int m_Int = 10;
		public int BonusInt;
		public int Int {
			get {
				return m_Int + BonusInt;
			}
			set {
				m_Int = value;
			}
		}
		public int ExpToNextLevel = 1000;
		int m_Exp = 0;
		public bool StatAllocationNeeded = false;
		public int Exp {
			get {
				return m_Exp;
			}
			set {
				m_Exp = value;
				if (m_Exp >= ExpToNextLevel) {
					LevelUp();
				}
			}
		}

		#endregion
		#region Constructors

		// delegate void BaseDelegate(Data data);

		//[field: NonSerialized]
		//public event BaseDelegate OnZeroHealth;
		[field: NonSerialized]
		event Action<Data> OnZeroHealthEvent = delegate { };
		public event Action<Data> OnZeroHealth {
			add {
				//Prevent double subscription
				OnZeroHealthEvent -= value;
				OnZeroHealthEvent += value;
			}
			remove {
				OnZeroHealthEvent -= value;
			}
		}

		protected Data() {
		}

		public int GetTickModifier() {

			return Dex * 10;
		}

		public Data(string username, Guid id) {

			Name = username;
			ID = id;
			Level = 1;
			Data.IDDataPairs.Add(id, this);
		}
		#endregion
		#region Instance Methods
		internal int GrantExperience() {

			return (int)(m_Str + m_Dex + m_Int + (int)((float)MaxHealth / 3));
		}

		private void LevelUp() {

			while (m_Exp < ExpToNextLevel) {
				m_Str++;
				m_Dex++;
				m_Int++;
				MaxHealth += 6;
				StatAllocationNeeded = true;
				Level++;
				ExpToNextLevel += (int)(ExpToNextLevel * 1.5f);
				thisBaseMobile.SendToClient(string.Format("You are now level {0}!", Level), Color.Cyan);
			}
		}
		#endregion
		#region Data Saving/Loading
		internal static void SaveData(params string[] paths) {

			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream;

			foreach (string path in paths) {
				stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
				switch (path) {
					case DataPaths.IdData:
						bf.Serialize(stream, IDDataPairs);
						break;
					case DataPaths.UserId:
						bf.Serialize(stream, UsernameIDPairs);
						break;
					case DataPaths.UserPwd:
						bf.Serialize(stream, UsernamePwdPairs);
						break;
					case DataPaths.World:
						bf.Serialize(stream, World.Rooms);
						break;
					case DataPaths.Spawn:
						bf.Serialize(stream, Data.NameSpawnPairs);
						break;
				}

				Console.WriteLine(string.Format("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
				stream.Close();
			}
		}

		internal static void LoadData() {

			Data.LoadData(
				DataPaths.IdData,
				DataPaths.Spawn,
				DataPaths.UserId,
				DataPaths.UserPwd,
				DataPaths.World);
		}

		internal static void LoadData(params string[] paths) {

			long bytes = 0;
			BinaryFormatter bf = new BinaryFormatter();
			Stream stream;

			foreach (string path in paths) {
				try {
					stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					switch (path) {
						case DataPaths.IdData:
							IDDataPairs = (Dictionary<Guid, Data>)bf.Deserialize(stream);
							break;
						case DataPaths.UserId:
							UsernameIDPairs = (Dictionary<string, Guid>)bf.Deserialize(stream);
							break;
						case DataPaths.UserPwd:
							UsernamePwdPairs = (Dictionary<string, string>)bf.Deserialize(stream);
							break;
						case DataPaths.World:
							World.Rooms = (Dictionary<string, Room>)bf.Deserialize(stream);
							break;
						case DataPaths.Spawn:
							NameSpawnPairs = (Dictionary<string, SpawnData>)bf.Deserialize(stream);
							break;
					}
					bytes += stream.Length;
					stream.Close();
				} catch (IOException e) {
					Console.WriteLine(e.Source);
					Console.WriteLine(e.Message);
					Data.SaveData(path);
				}
			}

			Console.WriteLine(string.Format("Loaded {0}kb of data into memory.", bytes / 1000f));


		}

		public static void SaveSpawnTemplates() {

			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream("spawn", FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, NameSpawnPairs);
			Console.WriteLine(string.Format("Saving {0}: {1}kb", stream.Name, stream.Length / 1000f));
			stream.Close();
		}
		#endregion


		public Data ShallowCopy() {

			return (Data)this.MemberwiseClone();
		}
	}
}


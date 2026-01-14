using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.IO;
using GameCore;
using GameCore.Util;

namespace GameCore {
	public class Data {

		#region Static Fields
		private static readonly object SaveLock = new object();
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string>();
		public static Dictionary<string, Guid> UsernameIDPairs = new Dictionary<string, Guid>();
		public static Dictionary<Guid, Data> IDDataPairs = new Dictionary<Guid, Data>();
		public static Dictionary<string, SpawnData> NameSpawnPairs = new Dictionary<string, SpawnData>();
		#endregion
		#region Exposed Data
		[ScriptIgnore]
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
					if (thisBaseMobile != null) {
						thisBaseMobile.Target = null;
						thisBaseMobile.InCombat = false;
					}
					OnZeroHealthEvent(this);
					try {
						if (thisBaseMobile != null) {
							thisBaseMobile.TriggerOnDeath(this);
						}
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
		public int ExpToNextLevel = 100;
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

		//[field: ScriptIgnore]
		//public event BaseDelegate OnZeroHealth;
		[field: ScriptIgnore]
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

		public Data() {
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

			while (m_Exp >= ExpToNextLevel) {
				m_Str++;
				m_Dex++;
				m_Int++;
				MaxHealth += 6;
				Level++;
				ExpToNextLevel += (int)(ExpToNextLevel * 1.5f);

				if (thisBaseMobile is PlayerEntity) {
					StatAllocationNeeded = true;
					thisBaseMobile.SendToClient(string.Format("You are now level {0}!", Level), Color.Cyan);
				}
			}
		}
		#endregion
		#region Data Saving/Loading
		internal static void SaveData(params string[] paths) {

			lock (SaveLock) {
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				StreamWriter writer;

				foreach (string path in paths) {
					string json = "";

					switch (path) {
						case DataPaths.IdData:
							Dictionary<string, Data> stringIdData = new Dictionary<string, Data>();
							foreach (var kvp in IDDataPairs) {
								stringIdData.Add(kvp.Key.ToString(), kvp.Value);
							}
							json = serializer.Serialize(stringIdData);
							break;
						case DataPaths.UserId:
							Dictionary<string, string> stringUserId = new Dictionary<string, string>();
							foreach (var kvp in UsernameIDPairs) {
								stringUserId.Add(kvp.Key, kvp.Value.ToString());
							}
							json = serializer.Serialize(stringUserId);
							break;
						case DataPaths.UserPwd:
							json = serializer.Serialize(UsernamePwdPairs);
							break;
						case DataPaths.World:
							json = serializer.Serialize(World.Rooms);
							break;
						case DataPaths.Spawn:
							json = serializer.Serialize(Data.NameSpawnPairs);
							break;
					}

					using (writer = new StreamWriter(path)) {
						writer.Write(json);
					}
					Console.WriteLine(string.Format("Saving {0}: {1}kb", path, json.Length / 1000f));
				}
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
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			StreamReader reader;

			foreach (string path in paths) {
				try {
					if (!File.Exists(path)) {
						continue;
					}

					string json;
					using (reader = new StreamReader(path)) {
						json = reader.ReadToEnd();
					}

					bytes += json.Length;

					switch (path) {
						case DataPaths.IdData:
							var rawIdData = serializer.Deserialize<Dictionary<string, Data>>(json) ?? new Dictionary<string, Data>();
							IDDataPairs = new Dictionary<Guid, Data>();
							foreach (var entry in rawIdData) {
								Guid id = new Guid(entry.Key);
								entry.Value.ID = id;
								IDDataPairs.Add(id, entry.Value);
							}
							break;
						case DataPaths.UserId:
							var rawUserId = serializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
							UsernameIDPairs = new Dictionary<string, Guid>();
							foreach (var entry in rawUserId) {
								UsernameIDPairs.Add(entry.Key, new Guid(entry.Value));
							}
							break;
						case DataPaths.UserPwd:
							UsernamePwdPairs = serializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
							break;
						case DataPaths.World:
							// Handle multiple map files
							if (File.Exists(DataPaths.MapList)) {
								string mapListJson = File.ReadAllText(DataPaths.MapList);
								var mapList = serializer.Deserialize<Dictionary<string, List<string>>>(mapListJson);
								if (mapList != null && mapList.ContainsKey("Maps")) {
									string mapDir = Path.GetDirectoryName(DataPaths.MapList);
									foreach (string mapFileName in mapList["Maps"]) {
										string mapFile = Path.Combine(mapDir, mapFileName);
										if (File.Exists(mapFile)) {
											string mapJson = File.ReadAllText(mapFile);
											var rooms = serializer.Deserialize<Dictionary<string, Room>>(mapJson);
											if (rooms != null) {
												foreach (var room in rooms) {
													if (!World.Rooms.ContainsKey(room.Key)) {
														// Attempt to reconstruct Location from key if it's missing or zeroed
														if (room.Value.Location == null || (room.Value.Location.X == 0 && room.Value.Location.Y == 0 && room.Value.Location.Z == 0)) {
															string[] coords = room.Key.Split(' ');
															if (coords.Length == 3) {
																if (int.TryParse(coords[0], out var x) && 
																	int.TryParse(coords[1], out var y) && 
																	int.TryParse(coords[2], out var z)) {
																	room.Value.Location = new Coordinate3(x, y, z);
																}
															}
														}
														World.Rooms.Add(room.Key, room.Value);
													}
												}
											}
										}
									}
								}
							} else {
								World.Rooms = serializer.Deserialize<Dictionary<string, Room>>(json) ?? new Dictionary<string, Room>();
								foreach (var room in World.Rooms) {
									if (room.Value.Location == null || (room.Value.Location.X == 0 && room.Value.Location.Y == 0 && room.Value.Location.Z == 0)) {
										string[] coords = room.Key.Split(' ');
										if (coords.Length == 3) {
											if (int.TryParse(coords[0], out var x) && 
												int.TryParse(coords[1], out var y) && 
												int.TryParse(coords[2], out var z)) {
												room.Value.Location = new Coordinate3(x, y, z);
											}
										}
									}
								}
							}

							foreach (var room in World.Rooms.Values) {
								if (room.EntitiesHere == null) room.EntitiesHere = new List<Guid>();
							}
							Console.WriteLine($"Loaded {World.Rooms.Count} rooms.");
							break;
						case DataPaths.Spawn:
							NameSpawnPairs = serializer.Deserialize<Dictionary<string, SpawnData>>(json) ?? new Dictionary<string, SpawnData>();
							foreach (var entry in NameSpawnPairs) {
								if (entry.Value.ID == Guid.Empty) entry.Value.ID = Guid.NewGuid(); // Template IDs shouldn't really matter but good to have
							}
							break;
					}
				} catch (IOException e) {
					Console.WriteLine(e.Source);
					Console.WriteLine(e.Message);
					Data.SaveData(path);
				}
			}

			Console.WriteLine($"Loaded {bytes / 1000f}kb of data into memory.");


		}

		public static void SaveSpawnTemplates() {

			lock (SaveLock) {
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				string json = serializer.Serialize(NameSpawnPairs);
				File.WriteAllText(DataPaths.Spawn, json);
				Console.WriteLine(string.Format("Saving {0}: {1}kb", DataPaths.Spawn, json.Length / 1000f));
			}
		}
		#endregion


		public Data ShallowCopy() {

			return (Data)this.MemberwiseClone();
		}
	}
}


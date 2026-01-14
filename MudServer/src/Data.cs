using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameCore;
using GameCore.Util;

namespace GameCore {
	public class Data {

		#region Static Fields
		private static readonly object SaveLock = new object();
		public static Dictionary<string, string> UsernamePwdPairs = new Dictionary<string, string>();
		public static Dictionary<string, Guid> UsernameIdPairs = new Dictionary<string, Guid>();
		public static Dictionary<Guid, Data> IdDataPairs = new Dictionary<Guid, Data>();
		public static Dictionary<string, SpawnData> NameSpawnPairs = new Dictionary<string, SpawnData>();
		#endregion
		#region Exposed Data
		[ScriptIgnore]
		public BaseMobile ThisBaseMobile;
		public string Name;
		public int Level;
		public Coordinate3 Location;
		public Guid Id;
		private int _mHealth;
		public int MaxHealth;

		public int Health {
			get => _mHealth;
			set {
				// If this object is dead, we can't make it more dead.
				if (_mHealth == 0 && value < 0) {
					return;
				}
				_mHealth = value;
				if (_mHealth > MaxHealth)
					_mHealth = MaxHealth;
				
				if (_mHealth > 0) return;
				
				_mHealth = 0;
				if (ThisBaseMobile != null) {
					ThisBaseMobile.Target = null;
					ThisBaseMobile.InCombat = false;
				}
				OnZeroHealthEvent(this);
				try {
					if (ThisBaseMobile != null) {
						ThisBaseMobile.TriggerOnDeath(this);
					}
				} catch (NullReferenceException) {

				}
			}
		}

		private int _str = 10;
		public int BonusStr;
		public int Str {
			get => _str + BonusStr;
			set => _str = value;
		}

		private int _dex = 10;
		public int BonusDex;
		public int Dex {
			get => _dex + BonusDex;
			set => _dex = value;
		}

		private int _int = 10;
		public int BonusInt;
		public int Int {
			get => _int + BonusInt;
			set => _int = value;
		}
		public int ExpToNextLevel = 100;
		private int _exp = 0;
		public bool StatAllocationNeeded = false;
		public int Exp {
			get => _exp;
			set {
				_exp = value;
				if (_exp >= ExpToNextLevel) {
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

		public Data() { }

		public int GetTickModifier() {

			return Dex * 10;
		}

		public Data(string username, Guid id) {

			Name = username;
			Id = id;
			Level = 1;
			Data.IdDataPairs.Add(id, this);
		}
		#endregion
		#region Instance Methods
		internal int GrantExperience() {

			return (int)(_str + _dex + _int + (int)((float)MaxHealth / 3));
		}

		private void LevelUp() {

			while (_exp >= ExpToNextLevel) {
				_str++;
				_dex++;
				_int++;
				MaxHealth += 6;
				Level++;
				ExpToNextLevel += (int)(ExpToNextLevel * 1.5f);

				if (ThisBaseMobile is PlayerEntity) {
					StatAllocationNeeded = true;
					ThisBaseMobile.SendToClient($"You are now level {Level}!", Color.Cyan);
				}
			}
		}
		#endregion
		#region Data Saving/Loading

		public static void SaveData(params string[] paths) {

			lock (SaveLock) {
				JavaScriptSerializer serializer = new JavaScriptSerializer();

				foreach (string path in paths) {
					string json = "";

					switch (path) {
						case DataPaths.IdData:
							Dictionary<string, Data> stringIdData = new Dictionary<string, Data>();
							foreach (var kvp in IdDataPairs) {
								stringIdData.Add(kvp.Key.ToString(), kvp.Value);
							}
							json = serializer.Serialize(stringIdData);
							break;
						case DataPaths.UserId:
							Dictionary<string, string> stringUserId = new Dictionary<string, string>();
							foreach (var kvp in UsernameIdPairs) {
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

					StreamWriter writer;
					using (writer = new StreamWriter(path)) {
						writer.Write(json);
					}
					Console.WriteLine($"Saving {path}: {json.Length / 1000f}kb");
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

		public static void LoadData(params string[] paths) {

			long bytes = 0;
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			foreach (string path in paths) {
				try {
					if (!File.Exists(path)) {
						continue;
					}

					string json;
					StreamReader reader;
					using (reader = new StreamReader(path)) {
						json = reader.ReadToEnd();
					}

					bytes += json.Length;

					switch (path) {
						case DataPaths.IdData:
							Dictionary<string, Data> rawIdData = serializer.Deserialize<Dictionary<string, Data>>(json) ?? new Dictionary<string, Data>();
							IdDataPairs = new Dictionary<Guid, Data>();
							foreach (KeyValuePair<string, Data> entry in rawIdData) {
								Guid id = new Guid(entry.Key);
								entry.Value.Id = id;
								IdDataPairs.Add(id, entry.Value);
							}
							break;
						case DataPaths.UserId:
							Dictionary<string, string> rawUserId = serializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
							UsernameIdPairs = new Dictionary<string, Guid>();
							foreach (KeyValuePair<string, string> entry in rawUserId) {
								UsernameIdPairs.Add(entry.Key, new Guid(entry.Value));
							}
							break;
						case DataPaths.UserPwd:
							UsernamePwdPairs = serializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
							break;
						case DataPaths.World:
							// Handle multiple map files
							if (File.Exists(DataPaths.MapList)) {
								string mapListJson = File.ReadAllText(DataPaths.MapList);
								Dictionary<string, List<string>> mapList = serializer.Deserialize<Dictionary<string, List<string>>>(mapListJson);
								if (mapList != null && mapList.ContainsKey("Maps")) {
									string mapDir = Path.GetDirectoryName(DataPaths.MapList);
									foreach (string mapFileName in mapList["Maps"]) {
										string mapFile = Path.Combine(mapDir, mapFileName);
										if (!File.Exists(mapFile)) continue;
										string mapJson = File.ReadAllText(mapFile);
										Dictionary<string, Room> rooms = serializer.Deserialize<Dictionary<string, Room>>(mapJson);
										if (rooms == null) continue;
										foreach (KeyValuePair<string, Room> room in rooms) {
											if (World.Rooms.ContainsKey(room.Key)) continue;
											// Attempt to reconstruct Location from key if it's missing or zeroed
											if (room.Value.Location == null || (room.Value.Location.X == 0 && room.Value.Location.Y == 0 && room.Value.Location.Z == 0)) {
												string[] coords = room.Key.Split(' ');
												if (coords.Length == 3) {
													if (int.TryParse(coords[0], out int x) && 
													    int.TryParse(coords[1], out int y) && 
													    int.TryParse(coords[2], out int z)) {
														room.Value.Location = new Coordinate3(x, y, z);
													}
												}
											}
											World.Rooms.Add(room.Key, room.Value);
										}
									}
								}
							} else {
								World.Rooms = serializer.Deserialize<Dictionary<string, Room>>(json) ?? new Dictionary<string, Room>();
								foreach (KeyValuePair<string, Room> room in World.Rooms) {
									if (room.Value.Location != null && (room.Value.Location.X != 0 ||
									                                    room.Value.Location.Y != 0 ||
									                                    room.Value.Location.Z != 0)) continue;
									string[] coords = room.Key.Split(' ');
									if (coords.Length != 3) continue;
									
									if (int.TryParse(coords[0], out int x) && 
									    int.TryParse(coords[1], out int y) && 
									    int.TryParse(coords[2], out int z)) {
										room.Value.Location = new Coordinate3(x, y, z);
									}
								}
							}

							foreach (Room room in World.Rooms.Values.Where(room => room.EntitiesHere == null)) {
								room.EntitiesHere = new List<Guid>();
							}
							Console.WriteLine($"Loaded {World.Rooms.Count} rooms.");
							break;
						case DataPaths.Spawn:
							NameSpawnPairs = serializer.Deserialize<Dictionary<string, SpawnData>>(json) ?? new Dictionary<string, SpawnData>();
							foreach (KeyValuePair<string, SpawnData> entry in NameSpawnPairs.Where(entry => entry.Value.Id == Guid.Empty)) {
								entry.Value.Id = Guid.NewGuid(); // Template IDs shouldn't really matter but good to have
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
				Console.WriteLine($"Saving {DataPaths.Spawn}: {json.Length / 1000f}kb");
			}
		}
		#endregion


		public Data ShallowCopy() {

			return (Data)this.MemberwiseClone();
		}
	}
}


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

		public static void SaveRoom(Room room) {

			if (room == null || string.IsNullOrEmpty(room.MapName)) {
				return;
			}

			SaveMap(room.MapName);
		}

		public static void SaveMap(string mapName) {

			if (string.IsNullOrEmpty(mapName)) return;

			lock (SaveLock) {
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				string mapDir = Path.GetDirectoryName(DataPaths.MapList);
				if (!string.IsNullOrEmpty(mapDir) && !Directory.Exists(mapDir)) {
					Directory.CreateDirectory(mapDir);
				}
				string mapFile = Path.Combine(mapDir, mapName);

				Dictionary<string, Room> mapRooms = new Dictionary<string, Room>();
				foreach (var kvp in World.Rooms) {
					if (kvp.Value.MapName == mapName) {
						mapRooms.Add(kvp.Key, kvp.Value);
					}
				}

				if (mapRooms.Count == 0 && File.Exists(mapFile)) {
					// Map might have been emptied, but usually we don't want to delete the file here
					// unless we explicitly want to allow deleting maps.
					return;
				}

				string updatedJson = serializer.Serialize(mapRooms);
				File.WriteAllText(mapFile, updatedJson);
				Console.WriteLine($"Saved map {mapName} ({mapRooms.Count} rooms)");
			}
		}

		public static void SaveAllMaps() {
			HashSet<string> mapNames = new HashSet<string>();
			lock (World.Rooms) {
				foreach (var room in World.Rooms.Values) {
					if (!string.IsNullOrEmpty(room.MapName)) {
						mapNames.Add(room.MapName);
					}
				}
			}

			foreach (var mapName in mapNames) {
				SaveMap(mapName);
			}
		}

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
							// We no longer save to world.json.
							// Rooms are now saved into individual map files via the Content Creator tool.
							break;
						case DataPaths.Creatures:
							json = serializer.Serialize(Data.NameSpawnPairs);
							break;
					}

					if (string.IsNullOrEmpty(json)) {
						if (path == DataPaths.World) {
							Console.WriteLine($"Skipping empty/no-op save for {path}");
							continue;
						}
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
				DataPaths.Creatures,
				DataPaths.UserId,
				DataPaths.UserPwd);
			
			// Load the world last as it depends on MapList/Individual maps or world.json fallback
			Data.LoadData(DataPaths.World);
		}

		public static void LoadData(params string[] paths) {

			long bytes = 0;
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			foreach (string path in paths) {
				try {
					string json = "";
					if (File.Exists(path)) {
						StreamReader reader;
						using (reader = new StreamReader(path)) {
							json = reader.ReadToEnd();
						}

						bytes += json.Length;
					} else if (path != DataPaths.World) {
						continue;
					}

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
							// Clear existing rooms for reload
							World.Rooms.Clear();
							World.Spawners.Clear();
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
											room.Value.MapName = mapFileName;
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

											// Assign room location to spawners AFTER room location is reconstructed
											if (room.Value.SpawnersHere != null) {
												foreach (var spawner in room.Value.SpawnersHere) {
													spawner.Location = room.Value.Location;
													if (!World.Spawners.Contains(spawner)) {
														World.Spawners.Add(spawner);
													}
												}
												World.StartAiThread();
											}

											World.Rooms.Add(room.Key, room.Value);
										}
									}
								}
							} else {
								World.Rooms = serializer.Deserialize<Dictionary<string, Room>>(json) ?? new Dictionary<string, Room>();
								foreach (KeyValuePair<string, Room> room in World.Rooms) {
									if (room.Value.Location == null || (room.Value.Location.X == 0 &&
									                                    room.Value.Location.Y == 0 &&
									                                    room.Value.Location.Z == 0)) {
										string[] coords = room.Key.Split(' ');
										if (coords.Length == 3) {
											if (int.TryParse(coords[0], out int x) && 
											    int.TryParse(coords[1], out int y) && 
											    int.TryParse(coords[2], out int z)) {
												room.Value.Location = new Coordinate3(x, y, z);
											}
										}
									}

									// Assign room location to spawners
									if (room.Value.SpawnersHere != null) {
										foreach (var spawner in room.Value.SpawnersHere) {
											spawner.Location = room.Value.Location;
											if (!World.Spawners.Contains(spawner)) {
												World.Spawners.Add(spawner);
											}
										}
										World.StartAiThread();
									}
								}
							}

							foreach (Room room in World.Rooms.Values.Where(room => room.EntitiesHere == null)) {
								room.EntitiesHere = new List<Guid>();
							}
							Console.WriteLine($"Loaded {World.Rooms.Count} rooms.");
							break;
						case DataPaths.Creatures:
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
				File.WriteAllText(DataPaths.Creatures, json);
				Console.WriteLine($"Saving {DataPaths.Creatures}: {json.Length / 1000f}kb");
			}
		}
		#endregion


		public Data ShallowCopy() {

			return (Data)this.MemberwiseClone();
		}
	}
}


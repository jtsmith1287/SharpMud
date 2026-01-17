using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Web.Script.Serialization;
using MudServer.Entity;
using MudServer.World;
using MudServer.Enums;
using MudServer.Server;

namespace MudServer.Util {
	public partial class ContentCreator : Form {

		SpawnData ActiveTemplate;
		private Dictionary<string, Room> _currentMapRooms;
		private Room _selectedRoom;
		private string _currentMapFile;
		private JavaScriptSerializer _serializer = new JavaScriptSerializer();

		public ContentCreator() {

			InitializeComponent();
			RefreshMobList();
			PopulateDropDownLists();
			LoadMapFiles();
			TabPageSpawnEditor.Text = "Spawn Editor";
		}

		private void LoadMapFiles() {
			ComboMapFiles.Items.Clear();
			if (File.Exists(DataPaths.MapList)) {
				string json = File.ReadAllText(DataPaths.MapList);
				var mapListData = _serializer.Deserialize<Dictionary<string, List<string>>>(json);
				if (mapListData != null && mapListData.ContainsKey("Maps")) {
					foreach (var mapFile in mapListData["Maps"]) {
						ComboMapFiles.Items.Add(mapFile);
					}
				}
			}
		}

		private void ComboMapFiles_SelectedIndexChanged(object sender, EventArgs e) {
			string mapFile = ComboMapFiles.SelectedItem.ToString();
			_currentMapFile = Path.Combine("maps", mapFile);
			if (File.Exists(_currentMapFile)) {
				string json = File.ReadAllText(_currentMapFile);
				_currentMapRooms = _serializer.Deserialize<Dictionary<string, Room>>(json);

				if (_currentMapRooms != null) {
					// Synchronize with World.Rooms to ensure we use the same objects
					// This allows AnsiMap (which uses MudServer.World.World.GetRoom) to see our changes immediately.
					var keys = _currentMapRooms.Keys.ToList();
					foreach (var key in keys) {
						Room loadedRoom = _currentMapRooms[key];
						// Reconstruct Location if it was lost or zeroed during deserialization
						if (loadedRoom.Location == null || (loadedRoom.Location.X == 0 && loadedRoom.Location.Y == 0 && loadedRoom.Location.Z == 0)) {
							string[] coords = key.Split(' ');
							if (coords.Length == 3 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y) && int.TryParse(coords[2], out int z)) {
								loadedRoom.Location = new Coordinate3(x, y, z);
							}
						}

						Room worldRoom = MudServer.World.World.GetRoom(loadedRoom.Location);
						if (worldRoom != null) {
							_currentMapRooms[key] = worldRoom;
						} else {
							MudServer.World.World.AddRoom(loadedRoom);
						}
					}
				}

				RefreshRoomList();
			}
		}

		private void RefreshRoomList() {
			ListRooms.Items.Clear();
			if (_currentMapRooms == null) return;

			foreach (var kvp in _currentMapRooms) {
				string name = kvp.Value.Name;
				if (IsEntryRoom(kvp.Value)) {
					name = "*" + name;
				}
				
				ListViewItem item = new ListViewItem(name);
				item.SubItems.Add(kvp.Key);
				item.Tag = kvp.Key;
				ListRooms.Items.Add(item);
			}
		}

		private bool IsEntryRoom(Room room) {
			if (room.ConnectedRooms == null) return false;
			if (room.IsEntryRoom) return true;
			foreach (var exit in room.ConnectedRooms.Values) {
				// If the exit coordinate is not in the current map rooms, it might be an entry room.
				// However, they might be in the same map file.
				// Another way is to look at the room name or some property.
				// For now, let's assume if it connects to a coordinate not in _currentMapRooms, it's an entry.
				string key = exit.X + " " + exit.Y + " " + exit.Z;
				if (!_currentMapRooms.ContainsKey(key)) return true;
			}
			return false;
		}

		private void ListRooms_SelectedIndexChanged(object sender, EventArgs e) {
			if (ListRooms.SelectedItems.Count == 0) return;
			string coordKey = ListRooms.SelectedItems[0].Tag as string;
			if (coordKey != null && _currentMapRooms.TryGetValue(coordKey, out _selectedRoom)) {
				UpdateRoomDetails();
			}
		}

		private void ListRooms_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				ListViewItem item = ListRooms.GetItemAt(e.X, e.Y);
				if (item != null) {
					item.Selected = true;
					ContextMenuRooms.Show(ListRooms, e.Location);
				}
			}
		}

		private void MenuItemDeleteRoom_Click(object sender, EventArgs e) {
			if (_selectedRoom == null) return;

			// Confirm deletion
			string message = $"Are you sure you want to delete the room '{_selectedRoom.Name}' at ({_selectedRoom.Location.X} {_selectedRoom.Location.Y} {_selectedRoom.Location.Z})?\n" +
			                 "This will also delete all exits connecting to this room.";

			// Store original state for potential restoration during reachability check
			var originalMap = new Dictionary<string, Room>(_currentMapRooms);
			string roomKey = _selectedRoom.Location.X + " " + _selectedRoom.Location.Y + " " + _selectedRoom.Location.Z;

			// Identify rooms that connect TO this room
			var connectionsToRoom = new List<Tuple<Room, string>>();
			foreach (var room in _currentMapRooms.Values) {
				if (room == _selectedRoom) continue;
				foreach (var exit in room.ConnectedRooms.ToList()) {
					if (exit.Value == _selectedRoom.Location) {
						connectionsToRoom.Add(new Tuple<Room, string>(room, exit.Key));
					}
				}
			}

			// Simulate deletion for reachability check
			_currentMapRooms.Remove(roomKey);
			foreach (var conn in connectionsToRoom) {
				conn.Item1.ConnectedRooms.Remove(conn.Item2);
			}

			List<string> unreachable;
			bool reachable = IsMapReachable(_currentMapRooms, out unreachable);

			if (!reachable) {
				message += "\n\nWARNING: Deleting this room will make the following rooms unreachable from entry points:\n" +
				           string.Join("\n", unreachable.Take(5)) + (unreachable.Count > 5 ? "\n..." : "");
			}

			var result = MessageBox.Show(message, "Confirm Room Deletion",
				MessageBoxButtons.YesNo,
				reachable ? MessageBoxIcon.Question : MessageBoxIcon.Warning);

			if (result == DialogResult.Yes) {
				// Find an adjacent room to select before deleting
				Coordinate3 adjacentToSelect = null;
				var directions = new List<Coordinate3> {
					new Coordinate3(0, 1, 0), new Coordinate3(0, -1, 0),
					new Coordinate3(1, 0, 0), new Coordinate3(-1, 0, 0),
					new Coordinate3(0, 0, 1), new Coordinate3(0, 0, -1)
				};

				foreach (var dir in directions) {
					Coordinate3 checkCoord = _selectedRoom.Location + dir;
					string checkKey = checkCoord.X + " " + checkCoord.Y + " " + checkCoord.Z;
					if (_currentMapRooms.ContainsKey(checkKey)) {
						adjacentToSelect = checkCoord;
						break;
					}
				}

				// Permanently remove from World too
				string stringCoord = string.Format("{0} {1} {2}", _selectedRoom.Location.X, _selectedRoom.Location.Y, _selectedRoom.Location.Z);
				MudServer.World.World.Rooms.Remove(stringCoord);

				_selectedRoom = null;
				TextRoomName.Text = "";
				TextRoomDesc.Text = "";
				TextRoomSpawners.Text = "";
				PanelExits.Controls.Clear();
				
				RefreshRoomList();
				UpdateAnsiMap();
				UpdateButtonAddExitText();

				// Select adjacent room if found
				if (adjacentToSelect != null) {
					SelectRoomByCoordinate(adjacentToSelect);
				}
			} else {
				// Restore
				_currentMapRooms = originalMap;
				foreach (var conn in connectionsToRoom) {
					conn.Item1.ConnectedRooms[conn.Item2] = originalMap[roomKey].Location;
				}
			}
		}

		private void UpdateRoomDetails() {
			if (_selectedRoom == null) return;
			TextRoomName.Text = _selectedRoom.Name;
			TextRoomDesc.Text = _selectedRoom.Description;
			
			// Spawners as text for now
			RefreshSpawnerList();
			
			RefreshExitList();
			UpdateAnsiMap();

			// Update Add Exit button text based on availability of adjacent rooms
			UpdateButtonAddExitText();
		}

		private void UpdateButtonAddExitText() {
			if (_selectedRoom == null) return;

			var directions = new Dictionary<string, Coordinate3> {
				{ "north", new Coordinate3(0, 1, 0) },
				{ "south", new Coordinate3(0, -1, 0) },
				{ "east", new Coordinate3(1, 0, 0) },
				{ "west", new Coordinate3(-1, 0, 0) },
				{ "up", new Coordinate3(0, 0, 1) },
				{ "down", new Coordinate3(0, 0, -1) }
			};

			bool foundAdjacentRoomToConnect = false;
			bool foundEmptySpaceForNewRoom = false;
			foreach (var dir in directions) {
				Coordinate3 adjCoord = _selectedRoom.Location + dir.Value;
				Room adjRoom = MudServer.World.World.GetRoom(adjCoord);
				if (adjRoom != null) {
					if (!_selectedRoom.ConnectedRooms.ContainsKey(dir.Key)) {
						foundAdjacentRoomToConnect = true;
					}
				} else {
					foundEmptySpaceForNewRoom = true;
				}
			}

			ButtonAddExit.Enabled = foundAdjacentRoomToConnect;
			ButtonAddNewRoom.Enabled = foundEmptySpaceForNewRoom;
		}

		private void RefreshSpawnerList() {
			PanelSpawners.Controls.Clear();
			if (_selectedRoom == null) return;

			foreach (var spawner in _selectedRoom.SpawnersHere) {
				string mobNames = string.Join(", ", spawner.SpawnDataIds.Select(id => {
					var data = DataManager.NameSpawnPairs.Values.FirstOrDefault(s => s.Id == id);
					return data != null ? $"{data.Name} ({data.Level})" : "Unknown";
				}));

				Panel spawnerItem = new Panel {
					Width = PanelSpawners.Width - 25,
					Height = 35,
					BackColor = System.Drawing.Color.Transparent
				};

				Label lblSpawner = new Label {
					Text = mobNames,
					Location = new Point(5, 8),
					AutoSize = true,
					ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
					Font = new Font("Segoe UI", 9)
				};

				Button btnDelete = new Button {
					Text = "X",
					Location = new Point(spawnerItem.Width - 30, 5),
					Size = new Size(25, 25),
					Tag = spawner,
					BackColor = System.Drawing.Color.FromArgb(63, 63, 70),
					ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 8, FontStyle.Bold)
				};
				btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 100, 100);

				btnDelete.Click += (s, e) => {
					var sp = (Spawner)((Control)s).Tag;
					_selectedRoom.SpawnersHere.Remove(sp);
					MudServer.World.World.Spawners.Remove(sp);
					RefreshSpawnerList();
				};

				spawnerItem.Controls.Add(lblSpawner);
				spawnerItem.Controls.Add(btnDelete);
				PanelSpawners.Controls.Add(spawnerItem);
			}
		}

		private void ButtonAddSpawner_Click(object sender, EventArgs e) {
			if (_selectedRoom == null) return;

			Form addSpawnerForm = new Form {
				Text = "Add Spawner",
				Size = new Size(300, 400),
				StartPosition = FormStartPosition.CenterParent,
				BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241)
			};

			CheckedListBox checkedListBox = new CheckedListBox {
				Dock = DockStyle.Top,
				Height = 300,
				BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				BorderStyle = BorderStyle.FixedSingle
			};

			foreach (var spawnData in DataManager.NameSpawnPairs.Values) {
				checkedListBox.Items.Add(spawnData);
			}
			// Use ToString() by setting DisplayMember to empty string or null, 
			// since we overrode ToString in SpawnData
			checkedListBox.DisplayMember = "";

			Button btnOk = new Button {
				Text = "OK",
				DialogResult = DialogResult.OK,
				Dock = DockStyle.Bottom,
				BackColor = System.Drawing.Color.FromArgb(63, 63, 70),
				FlatStyle = FlatStyle.Flat
			};

			addSpawnerForm.Controls.Add(checkedListBox);
			addSpawnerForm.Controls.Add(btnOk);

			if (addSpawnerForm.ShowDialog() == DialogResult.OK) {
				List<Guid> selectedIds = new List<Guid>();
				foreach (SpawnData item in checkedListBox.CheckedItems) {
					selectedIds.Add(item.Id);
				}

				if (selectedIds.Count > 0) {
					new Spawner(_selectedRoom, selectedIds);
					RefreshSpawnerList();
				}
			}
		}

		private void RefreshExitList() {
			PanelExits.Controls.Clear();
			if (_selectedRoom == null) return;

			foreach (var exit in _selectedRoom.ConnectedRooms) {
				string direction = exit.Key;
				Coordinate3 targetCoord = exit.Value;
				Room targetRoom = MudServer.World.World.GetRoom(targetCoord);
				string targetName = targetRoom != null ? targetRoom.Name : "Unknown Room";

				Panel exitItem = new Panel { 
					Width = PanelExits.Width - 25, 
					Height = 35, 
					BackColor = System.Drawing.Color.Transparent 
				};
				
				LinkLabel lblExit = new LinkLabel { 
					Text = $"{direction} ({targetName})", 
					Location = new Point(5, 8), 
					AutoSize = true,
					Tag = targetCoord,
					LinkColor = System.Drawing.Color.FromArgb(0, 122, 204),
					ActiveLinkColor = System.Drawing.Color.FromArgb(0, 150, 250),
					VisitedLinkColor = System.Drawing.Color.FromArgb(0, 122, 204),
					Font = new Font("Segoe UI", 9, FontStyle.Bold)
				};
				lblExit.LinkClicked += (s, e) => {
					SelectRoomByCoordinate((Coordinate3)((Control)s).Tag);
				};

				Button btnDelete = new Button { 
					Text = "X", 
					Location = new Point(exitItem.Width - 30, 5), 
					Size = new Size(25, 25),
					Tag = direction,
					BackColor = System.Drawing.Color.FromArgb(63, 63, 70),
					ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
					FlatStyle = FlatStyle.Flat,
					Font = new Font("Segoe UI", 8, FontStyle.Bold)
				};
				btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 100, 100);
				
				btnDelete.Click += (s, e) => {
					ConfirmAndDeleteExit((string)((Control)s).Tag);
				};

				exitItem.Controls.Add(lblExit);
				exitItem.Controls.Add(btnDelete);
				PanelExits.Controls.Add(exitItem);
			}
		}

		private void SelectRoomByCoordinate(Coordinate3 coord) {
			string key = coord.X + " " + coord.Y + " " + coord.Z;
			
			// Check if it's in the current map first
			foreach (ListViewItem item in ListRooms.Items) {
				if ((string)item.Tag == key) {
					item.Selected = true;
					item.EnsureVisible();
					return;
				}
			}

			// If not in current map, check if it exists in World and belongs to another map
			Room targetRoom = MudServer.World.World.GetRoom(coord);
			if (targetRoom != null && !string.IsNullOrEmpty(targetRoom.MapName)) {
				// Switch map
				for (int i = 0; i < ComboMapFiles.Items.Count; i++) {
					if (ComboMapFiles.Items[i].ToString() == targetRoom.MapName) {
						ComboMapFiles.SelectedIndex = i;
						ComboMapFiles_SelectedIndexChanged(ComboMapFiles, EventArgs.Empty);
						
						// Now that the map is switched, the room list is refreshed, find the room again.
						foreach (ListViewItem item in ListRooms.Items) {
							if ((string)item.Tag == key) {
								item.Selected = true;
								item.EnsureVisible();
								return;
							}
						}
						break;
					}
				}
			}
			
			MessageBox.Show("Room is not in the current map view and could not be found in other maps.");
		}

		private void UpdateAnsiMap() {
			if (_selectedRoom == null) return;
			string mapText = AnsiMap.Display(_selectedRoom.Location);
			AppendAnsiToRichTextBox(RichTextAnsiMap, mapText);
		}

		private void AppendAnsiToRichTextBox(RichTextBox rtb, string text) {
			rtb.Clear();
			string[] parts = System.Text.RegularExpressions.Regex.Split(text, @"(\x1b\[[0-9;]*m)");
			System.Drawing.Color currentColor = System.Drawing.Color.White;
			
			foreach (var part in parts) {
				if (part.StartsWith("\x1b[")) {
					if (part == MudServer.Util.Color.Reset) currentColor = System.Drawing.Color.White;
					else if (part == MudServer.Util.Color.Red) currentColor = System.Drawing.Color.Red;
					else if (part == MudServer.Util.Color.Green) currentColor = System.Drawing.Color.Green;
					else if (part == MudServer.Util.Color.Yellow) currentColor = System.Drawing.Color.Yellow;
					else if (part == MudServer.Util.Color.Blue) currentColor = System.Drawing.Color.Blue;
					else if (part == MudServer.Util.Color.Cyan) currentColor = System.Drawing.Color.Cyan;
					else if (part == MudServer.Util.Color.Magenta) currentColor = System.Drawing.Color.Magenta;
					// Add more as needed
				} else {
					rtb.SelectionStart = rtb.TextLength;
					rtb.SelectionLength = 0;
					rtb.SelectionColor = currentColor;
					rtb.AppendText(part);
				}
			}
		}

		private void ButtonNorth_Click(object sender, EventArgs e) { TryMove(0, 1, 0); }
		private void ButtonSouth_Click(object sender, EventArgs e) { TryMove(0, -1, 0); }
		private void ButtonEast_Click(object sender, EventArgs e) { TryMove(1, 0, 0); }
		private void ButtonWest_Click(object sender, EventArgs e) { TryMove(-1, 0, 0); }
		private void ButtonUp_Click(object sender, EventArgs e) { TryMove(0, 0, 1); }
		private void ButtonDown_Click(object sender, EventArgs e) { TryMove(0, 0, -1); }

		private void TryMove(int dx, int dy, int dz) {
			if (_selectedRoom == null) return;
			Coordinate3 newCoord = new Coordinate3(_selectedRoom.Location.X + dx, _selectedRoom.Location.Y + dy, _selectedRoom.Location.Z + dz);
			
			SelectRoomByCoordinate(newCoord);
		}

		private void ButtonSaveAll_Click(object sender, EventArgs e) {
			if (_selectedRoom != null) {
				// Save current room details from fields before saving file
				_selectedRoom.Name = TextRoomName.Text;
				_selectedRoom.Description = TextRoomDesc.Text;
				// Parsing back Spawners
				try {
					_selectedRoom.SpawnersHere = _serializer.Deserialize<List<Spawner>> (TextRoomSpawners.Text);
				} catch {
					MessageBox.Show("Error parsing Spawners JSON.");
				}
			}

			if (_currentMapRooms != null && !string.IsNullOrEmpty(_currentMapFile)) {
				string json = _serializer.Serialize(_currentMapRooms);
				File.WriteAllText(_currentMapFile, json);
				MessageBox.Show("Map saved and server notified.");
				
				// Alert the server through an event
				OnMapSaved?.Invoke(null, EventArgs.Empty);
			}
		}

		private void ConfirmAndDeleteExit(string direction) {
			if (_selectedRoom == null) return;
			if (!_selectedRoom.ConnectedRooms.ContainsKey(direction)) return;

			Coordinate3 targetCoord = _selectedRoom.ConnectedRooms[direction];
			Room targetRoom = MudServer.World.World.GetRoom(targetCoord);
			string targetName = targetRoom != null ? targetRoom.Name : "Unknown Room";

			// Identify rooms that will be disconnected
			string message = $"Are you sure you want to delete the exit '{direction}' to {targetName}?\n" +
			                 $"This will also delete the return exit from {targetName}.";

			// Temporary delete for reachability check
			var originalExits = new Dictionary<string, Coordinate3>(_selectedRoom.ConnectedRooms);
			_selectedRoom.ConnectedRooms.Remove(direction);

			// Mutual removal from target room if it exists
			Dictionary<string, Coordinate3> targetOriginalExits = null;
			string returnDirection = "";
			if (targetRoom != null) {
				targetOriginalExits = new Dictionary<string, Coordinate3>(targetRoom.ConnectedRooms);
				foreach (var kvp in targetRoom.ConnectedRooms) {
					if (kvp.Value == _selectedRoom.Location) {
						returnDirection = kvp.Key;
						break;
					}
				}
				if (!string.IsNullOrEmpty(returnDirection)) {
					targetRoom.ConnectedRooms.Remove(returnDirection);
				}
			}

			List<string> unreachable;
			bool reachable = IsMapReachable(_currentMapRooms, out unreachable);

			if (!reachable) {
				message += "\n\nAGGRESSIVE WARNING: Deleting this exit will make the following rooms unreachable from entry points:\n" + 
				           string.Join("\n", unreachable.Take(5)) + (unreachable.Count > 5 ? "\n..." : "");
			}

			var result = MessageBox.Show(message, "Confirm Deletion", 
				MessageBoxButtons.YesNo, 
				reachable ? MessageBoxIcon.Question : MessageBoxIcon.Warning);

			if (result == DialogResult.Yes) {
				RefreshExitList();
				UpdateAnsiMap();
			} else {
				// Restore
				_selectedRoom.ConnectedRooms = originalExits;
				if (targetRoom != null && targetOriginalExits != null) {
					targetRoom.ConnectedRooms = targetOriginalExits;
				}
			}
		}

		private bool IsMapReachable(Dictionary<string, Room> map, out List<string> unreachableRooms) {
			unreachableRooms = new List<string>();
			if (map == null || map.Count == 0) return true;

			HashSet<string> visited = new HashSet<string>();
			Queue<string> queue = new Queue<string>();

			// Find entry rooms
			var entryRooms = map.Values.Where(r => r.IsEntryRoom || IsEntryRoom(r)).ToList();
			if (entryRooms.Count == 0) {
				// If no explicit entry rooms, start from the first one in the map list
				// but exclude Purgatory if it happens to be the first one
				var firstRoom = map.Values.FirstOrDefault(r => !(r.Location.X == int.MaxValue && r.Location.Y == int.MaxValue && r.Location.Z == int.MaxValue));
				if (firstRoom != null) {
					entryRooms.Add(firstRoom);
				}
			}

			foreach (var entry in entryRooms) {
				string key = entry.Location.X + " " + entry.Location.Y + " " + entry.Location.Z;
				if (!visited.Contains(key)) {
					visited.Add(key);
					queue.Enqueue(key);
				}
			}

			while (queue.Count > 0) {
				string currentKey = queue.Dequeue();
				if (!map.TryGetValue(currentKey, out Room currentRoom)) continue;

				foreach (var exit in currentRoom.ConnectedRooms.Values) {
					string nextKey = exit.X + " " + exit.Y + " " + exit.Z;
					if (map.ContainsKey(nextKey) && !visited.Contains(nextKey)) {
						visited.Add(nextKey);
						queue.Enqueue(nextKey);
					}
				}
			}

			foreach (var key in map.Keys) {
				Room room = map[key];
				// Skip Purgatory - it's expected to be orphaned
				if (room.Location.X == int.MaxValue && room.Location.Y == int.MaxValue && room.Location.Z == int.MaxValue) {
					continue;
				}

				if (!visited.Contains(key)) {
					unreachableRooms.Add(room.Name + " (" + key + ")");
				}
			}

			return unreachableRooms.Count == 0;
		}

		private void ButtonAddExit_Click(object sender, EventArgs e) {
			if (_selectedRoom == null) return;

			Form addDialog = new Form {
				Text = "Add Exit",
				Size = new Size(450, 400),
				FormBorderStyle = FormBorderStyle.FixedDialog,
				StartPosition = FormStartPosition.CenterParent,
				BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				Font = new Font("Segoe UI", 9)
			};

			ListBox listOptions = new ListBox {
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				BorderStyle = BorderStyle.None,
				Font = new Font("Segoe UI", 10),
				ItemHeight = 25
			};

			var directions = new Dictionary<string, Coordinate3> {
				{ "north", new Coordinate3(0, 1, 0) },
				{ "south", new Coordinate3(0, -1, 0) },
				{ "east", new Coordinate3(1, 0, 0) },
				{ "west", new Coordinate3(-1, 0, 0) },
				{ "up", new Coordinate3(0, 0, 1) },
				{ "down", new Coordinate3(0, 0, -1) }
			};

			bool foundAny = false;
			foreach (var dir in directions) {
				Coordinate3 adjCoord = _selectedRoom.Location + dir.Value;
				Room adjRoom = MudServer.World.World.GetRoom(adjCoord);
				if (adjRoom != null) {
					if (!_selectedRoom.ConnectedRooms.ContainsKey(dir.Key)) {
						listOptions.Items.Add(new AdjRoomItem { 
							Direction = dir.Key, 
							Room = adjRoom, 
							Display = $"{dir.Key.ToUpper()}: {adjRoom.Name} ({adjRoom.Location.X} {adjRoom.Location.Y} {adjRoom.Location.Z})" 
						});
						foundAny = true;
					}
				}
			}

			if (!foundAny) {
				listOptions.Items.Add("No adjacent rooms found without existing exits.");
				listOptions.Enabled = false;
			}

			listOptions.DisplayMember = "Display";

			Button btnConfirm = new Button {
				Text = "Add Connection",
				Dock = DockStyle.Bottom,
				Height = 45,
				BackColor = System.Drawing.Color.FromArgb(63, 63, 70),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				FlatStyle = FlatStyle.Flat,
				Enabled = foundAny
			};
			btnConfirm.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 100, 100);

			btnConfirm.Click += (s, ev) => {
				if (listOptions.SelectedItem is AdjRoomItem selected) {
					string dir = selected.Direction;
					Room target = selected.Room;

					_selectedRoom.ConnectedRooms[dir] = target.Location;

					string reverseDir = GetReverseDirection(dir);
					if (!target.ConnectedRooms.ContainsKey(reverseDir)) {
						target.ConnectedRooms[reverseDir] = _selectedRoom.Location;
					}

					addDialog.DialogResult = DialogResult.OK;
					addDialog.Close();
				}
			};

			addDialog.Controls.Add(listOptions);
			addDialog.Controls.Add(btnConfirm);

			if (addDialog.ShowDialog() == DialogResult.OK) {
				RefreshExitList();
				UpdateAnsiMap();
				UpdateButtonAddExitText();
			}
		}

		private void ButtonAddNewRoom_Click(object sender, EventArgs e) {
			if (_selectedRoom == null) return;

			Form addDialog = new Form {
				Text = "Add New Room",
				Size = new Size(450, 400),
				FormBorderStyle = FormBorderStyle.FixedDialog,
				StartPosition = FormStartPosition.CenterParent,
				BackColor = System.Drawing.Color.FromArgb(45, 45, 48),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				Font = new Font("Segoe UI", 9)
			};

			ListBox listOptions = new ListBox {
				Dock = DockStyle.Fill,
				BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				BorderStyle = BorderStyle.None,
				Font = new Font("Segoe UI", 10),
				ItemHeight = 25
			};

			var directions = new Dictionary<string, Coordinate3> {
				{ "north", new Coordinate3(0, 1, 0) },
				{ "south", new Coordinate3(0, -1, 0) },
				{ "east", new Coordinate3(1, 0, 0) },
				{ "west", new Coordinate3(-1, 0, 0) },
				{ "up", new Coordinate3(0, 0, 1) },
				{ "down", new Coordinate3(0, 0, -1) }
			};

			bool foundAny = false;
			foreach (var dir in directions) {
				Coordinate3 targetCoord = _selectedRoom.Location + dir.Value;
				Room existingRoom = MudServer.World.World.GetRoom(targetCoord);
				if (existingRoom == null) {
					listOptions.Items.Add(new NewRoomOption {
						Direction = dir.Key,
						Coordinate = targetCoord,
						Display = $"New Room to the {dir.Key.ToUpper()} ({targetCoord.X} {targetCoord.Y} {targetCoord.Z})"
					});
					foundAny = true;
				}
			}

			if (!foundAny) {
				listOptions.Items.Add("No space for new rooms.");
				listOptions.Enabled = false;
			}

			listOptions.DisplayMember = "Display";

			Button btnConfirm = new Button {
				Text = "Create Room",
				Dock = DockStyle.Bottom,
				Height = 45,
				BackColor = System.Drawing.Color.FromArgb(63, 63, 70),
				ForeColor = System.Drawing.Color.FromArgb(241, 241, 241),
				FlatStyle = FlatStyle.Flat,
				Enabled = foundAny
			};
			btnConfirm.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(100, 100, 100);

			btnConfirm.Click += (s, ev) => {
				if (!(listOptions.SelectedItem is NewRoomOption selected)) return;
				
				Room newRoom = new Room(selected.Coordinate, "New Room");
				newRoom.Description = "A newly created room.";
				newRoom.MapName = ComboMapFiles.SelectedItem.ToString();
					
				// Add to current map
				string key = selected.Coordinate.X + " " + selected.Coordinate.Y + " " + selected.Coordinate.Z;
				_currentMapRooms[key] = newRoom;
					
				// Add mutual exits
				_selectedRoom.ConnectedRooms[selected.Direction] = newRoom.Location;
				newRoom.ConnectedRooms[GetReverseDirection(selected.Direction)] = _selectedRoom.Location;

				// Add to World too so GetRoom works
				MudServer.World.World.AddRoom(newRoom);

				// Refresh room list
				RefreshRoomList();
					
				addDialog.DialogResult = DialogResult.OK;
				addDialog.Close();
			};

			addDialog.Controls.Add(listOptions);
			addDialog.Controls.Add(btnConfirm);

			if (addDialog.ShowDialog() == DialogResult.OK) {
				RefreshExitList();
				UpdateAnsiMap();
				UpdateButtonAddExitText();
			}
		}

		private class AdjRoomItem {
			public string Direction;
			public Room Room;
			public string Display;
			public override string ToString() => Display;
		}

		private class NewRoomOption {
			public string Direction;
			public Coordinate3 Coordinate;
			public string Display;
			public override string ToString() => Display;
		}

		private string GetReverseDirection(string dir) {
			switch (dir.ToLower()) {
				case "north": return "south";
				case "south": return "north";
				case "east": return "west";
				case "west": return "east";
				case "up": return "down";
				case "down": return "up";
				default: return "";
			}
		}

		public static event EventHandler OnMapSaved;

		private void ButtonSubmit_Click(object sender, EventArgs e) {

			if (TextName.Text == "") {
				MessageBox.Show("You need to at least provide a name.",
					"Error!",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1);
				TextName.Focus();
				return;
			} else {
				if (DataManager.NameSpawnPairs.ContainsKey(TextName.Text)) {
					DialogResult result = MessageBox.Show("This entry exists. Overwite?.",
					"Warning!",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1);

					switch (result) {
						case DialogResult.Yes:
							ActiveTemplate = DataManager.NameSpawnPairs[TextName.Text];
							break;
						case DialogResult.No:
							return;
					}
				}
			}

			if (ActiveTemplate == null)
				ActiveTemplate = new SpawnData(TextName.Text);

			SetFormValuesToTemplate();

			try {
				DataManager.NameSpawnPairs.Add(ActiveTemplate.Name, ActiveTemplate);
			} catch (ArgumentException) {
				DataManager.NameSpawnPairs[TextName.Text] = ActiveTemplate;
			}

			DataManager.SaveSpawnTemplates();
			RefreshMobList();
			RefreshSpawnerList();
			ActiveTemplate = null;

		}


		private void SetFormValuesToTemplate() {

			ActiveTemplate.Name = TextName.Text;
			ActiveTemplate.Humanoid = CheckBoxHumanoid.Checked;
			ActiveTemplate.Level = (int)NumericStartingLevel.Value;
			ActiveTemplate.MaxHealth = (int)NumericHealth.Value;
			ActiveTemplate.Str = (int)NumericStr.Value;
			ActiveTemplate.Dex = (int)NumericDex.Value;
			ActiveTemplate.Int = (int)NumericInt.Value;
			ActiveTemplate.Behaviour = (Disposition)Enum.Parse(typeof(Disposition), ComboDisposition.SelectedItem.ToString());
			ActiveTemplate.Faction = (Taxonomy)Enum.Parse(typeof(Taxonomy), ComboFaction.SelectedItem.ToString());
		}

		private void SetTemplateValuesToForm() {

			TextName.Text = ActiveTemplate.Name;
			CheckBoxHumanoid.Checked = ActiveTemplate.Humanoid;
			NumericStartingLevel.Value = ActiveTemplate.Level;
			NumericHealth.Value = ActiveTemplate.MaxHealth;
			NumericStr.Value = ActiveTemplate.Str;
			NumericDex.Value = ActiveTemplate.Dex;
			NumericInt.Value = ActiveTemplate.Int;
			ComboDisposition.SelectedItem = Enum.GetName(typeof(Disposition), ActiveTemplate.Behaviour);
			ComboFaction.SelectedItem = Enum.GetName(typeof(Taxonomy), ActiveTemplate.Faction);

		}

		void RefreshMobList() {

			ComboMobList.Items.Clear();
			foreach (var entry in DataManager.NameSpawnPairs) {
				ComboMobList.Items.Add(entry.Key);
			}
		}

		private void ComboMobList_SelectionChangeCommitted(object sender, EventArgs e) {

			ActiveTemplate = DataManager.NameSpawnPairs[(string)ComboMobList.SelectedItem];
			SetTemplateValuesToForm();

		}

		private void PopulateDropDownLists() {

			ComboDisposition.DataSource = Enum.GetNames(typeof(Disposition));
			ComboFaction.DataSource = Enum.GetNames(typeof(Taxonomy));
		}
	}
}

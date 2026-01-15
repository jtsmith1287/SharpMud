namespace ServerCore {
	partial class ContentCreator {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.TextName = new System.Windows.Forms.TextBox();
			this.ButtonSubmit = new System.Windows.Forms.Button();
			this.LabelName = new System.Windows.Forms.Label();
			this.ComboMobList = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.NumericHealth = new System.Windows.Forms.NumericUpDown();
			this.NumericStr = new System.Windows.Forms.NumericUpDown();
			this.NumericDex = new System.Windows.Forms.NumericUpDown();
			this.NumericInt = new System.Windows.Forms.NumericUpDown();
			this.Health = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.ComboDisposition = new System.Windows.Forms.ComboBox();
			this.spawnDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.ComboFaction = new System.Windows.Forms.ComboBox();
			this.CheckBoxHumanoid = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.StartingLevel = new System.Windows.Forms.Label();
			this.NumericStartingLevel = new System.Windows.Forms.NumericUpDown();
			this.TabControlMain = new System.Windows.Forms.TabControl();
			this.TabPageMobMaker = new System.Windows.Forms.TabPage();
			this.TabPageRoomEditor = new System.Windows.Forms.TabPage();
			this.ComboMapFiles = new System.Windows.Forms.ComboBox();
			this.ListRooms = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ContextMenuRooms = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.MenuItemDeleteRoom = new System.Windows.Forms.ToolStripMenuItem();
			this.TextRoomName = new System.Windows.Forms.TextBox();
			this.TextRoomDesc = new System.Windows.Forms.TextBox();
			this.TextRoomSpawners = new System.Windows.Forms.TextBox();
			this.LabelRoomName = new System.Windows.Forms.Label();
			this.LabelRoomDesc = new System.Windows.Forms.Label();
			this.LabelRoomSpawners = new System.Windows.Forms.Label();
			this.LabelRoomExits = new System.Windows.Forms.Label();
			this.PanelExits = new System.Windows.Forms.FlowLayoutPanel();
			this.ButtonAddExit = new System.Windows.Forms.Button();
			this.ButtonAddNewRoom = new System.Windows.Forms.Button();
			this.RichTextAnsiMap = new System.Windows.Forms.RichTextBox();
			this.ButtonNorth = new System.Windows.Forms.Button();
			this.ButtonSouth = new System.Windows.Forms.Button();
			this.ButtonEast = new System.Windows.Forms.Button();
			this.ButtonWest = new System.Windows.Forms.Button();
			this.ButtonUp = new System.Windows.Forms.Button();
			this.ButtonDown = new System.Windows.Forms.Button();
			this.ButtonSaveAll = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.NumericHealth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericStr)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericDex)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericInt)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spawnDataBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericStartingLevel)).BeginInit();
			this.SuspendLayout();
			// 
			// TextName
			// 
			this.TextName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.TextName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.TextName.Location = new System.Drawing.Point(110, 53);
			this.TextName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.TextName.Name = "TextName";
			this.TextName.Size = new System.Drawing.Size(261, 23);
			this.TextName.TabIndex = 0;
			// 
			// ButtonSubmit
			// 
			this.ButtonSubmit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonSubmit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonSubmit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonSubmit.Location = new System.Drawing.Point(14, 16);
			this.ButtonSubmit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonSubmit.Name = "ButtonSubmit";
			this.ButtonSubmit.Size = new System.Drawing.Size(87, 30);
			this.ButtonSubmit.TabIndex = 1;
			this.ButtonSubmit.Text = "Submit";
			this.ButtonSubmit.UseVisualStyleBackColor = false;
			this.ButtonSubmit.Click += new System.EventHandler(this.ButtonSubmit_Click);
			// 
			// LabelName
			// 
			this.LabelName.AutoSize = true;
			this.LabelName.Location = new System.Drawing.Point(14, 57);
			this.LabelName.Name = "LabelName";
			this.LabelName.Size = new System.Drawing.Size(42, 17);
			this.LabelName.TabIndex = 2;
			this.LabelName.Text = "Name";
			// 
			// ComboMobList
			// 
			this.ComboMobList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ComboMobList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboMobList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ComboMobList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ComboMobList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ComboMobList.FormattingEnabled = true;
			this.ComboMobList.Location = new System.Drawing.Point(181, 17);
			this.ComboMobList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComboMobList.MaxDropDownItems = 12;
			this.ComboMobList.Name = "ComboMobList";
			this.ComboMobList.Size = new System.Drawing.Size(189, 23);
			this.ComboMobList.TabIndex = 3;
			this.ComboMobList.SelectionChangeCommitted += new System.EventHandler(this.ComboMobList_SelectionChangeCommitted);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(119, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 17);
			this.label1.TabIndex = 4;
			this.label1.Text = "Mob List";
			// 
			// NumericHealth
			// 
			this.NumericHealth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.NumericHealth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericHealth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.NumericHealth.Location = new System.Drawing.Point(107, 151);
			this.NumericHealth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericHealth.Name = "NumericHealth";
			this.NumericHealth.Size = new System.Drawing.Size(108, 23);
			this.NumericHealth.TabIndex = 5;
			this.NumericHealth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericStr
			// 
			this.NumericStr.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.NumericStr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericStr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.NumericStr.Location = new System.Drawing.Point(107, 185);
			this.NumericStr.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericStr.Name = "NumericStr";
			this.NumericStr.Size = new System.Drawing.Size(108, 23);
			this.NumericStr.TabIndex = 6;
			this.NumericStr.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericDex
			// 
			this.NumericDex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.NumericDex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericDex.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.NumericDex.Location = new System.Drawing.Point(107, 219);
			this.NumericDex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericDex.Name = "NumericDex";
			this.NumericDex.Size = new System.Drawing.Size(108, 23);
			this.NumericDex.TabIndex = 7;
			this.NumericDex.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericInt
			// 
			this.NumericInt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.NumericInt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericInt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.NumericInt.Location = new System.Drawing.Point(107, 253);
			this.NumericInt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericInt.Name = "NumericInt";
			this.NumericInt.Size = new System.Drawing.Size(108, 23);
			this.NumericInt.TabIndex = 8;
			this.NumericInt.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// Health
			// 
			this.Health.AutoSize = true;
			this.Health.Location = new System.Drawing.Point(15, 153);
			this.Health.Name = "Health";
			this.Health.Size = new System.Drawing.Size(45, 17);
			this.Health.TabIndex = 9;
			this.Health.Text = "Health";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 187);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 17);
			this.label3.TabIndex = 10;
			this.label3.Text = "Strength";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(15, 221);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 17);
			this.label4.TabIndex = 11;
			this.label4.Text = "Dexterity";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(15, 255);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(74, 17);
			this.label5.TabIndex = 12;
			this.label5.Text = "Intelligence";
			// 
			// ComboDisposition
			// 
			this.ComboDisposition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ComboDisposition.DataSource = this.spawnDataBindingSource;
			this.ComboDisposition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboDisposition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ComboDisposition.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ComboDisposition.FormattingEnabled = true;
			this.ComboDisposition.Location = new System.Drawing.Point(107, 288);
			this.ComboDisposition.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComboDisposition.Name = "ComboDisposition";
			this.ComboDisposition.Size = new System.Drawing.Size(108, 23);
			this.ComboDisposition.TabIndex = 13;
			// 
			// spawnDataBindingSource
			// 
			this.spawnDataBindingSource.DataSource = typeof(GameCore.Util.SpawnData);
			// 
			// ComboFaction
			// 
			this.ComboFaction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ComboFaction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboFaction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ComboFaction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ComboFaction.FormattingEnabled = true;
			this.ComboFaction.Location = new System.Drawing.Point(107, 324);
			this.ComboFaction.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComboFaction.Name = "ComboFaction";
			this.ComboFaction.Size = new System.Drawing.Size(108, 23);
			this.ComboFaction.TabIndex = 14;
			// 
			// CheckBoxHumanoid
			// 
			this.CheckBoxHumanoid.AutoSize = true;
			this.CheckBoxHumanoid.Checked = true;
			this.CheckBoxHumanoid.CheckState = System.Windows.Forms.CheckState.Checked;
			this.CheckBoxHumanoid.Location = new System.Drawing.Point(108, 89);
			this.CheckBoxHumanoid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.CheckBoxHumanoid.Name = "CheckBoxHumanoid";
			this.CheckBoxHumanoid.Size = new System.Drawing.Size(106, 21);
			this.CheckBoxHumanoid.TabIndex = 15;
			this.CheckBoxHumanoid.Text = "Yes if checked";
			this.CheckBoxHumanoid.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 90);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 17);
			this.label2.TabIndex = 16;
			this.label2.Text = "Humanoid?";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(15, 288);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(70, 17);
			this.label6.TabIndex = 17;
			this.label6.Text = "Disposition";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(15, 324);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(49, 17);
			this.label7.TabIndex = 18;
			this.label7.Text = "Faction";
			// 
			// StartingLevel
			// 
			this.StartingLevel.AutoSize = true;
			this.StartingLevel.Location = new System.Drawing.Point(15, 120);
			this.StartingLevel.Name = "StartingLevel";
			this.StartingLevel.Size = new System.Drawing.Size(86, 17);
			this.StartingLevel.TabIndex = 20;
			this.StartingLevel.Text = "Starting Level";
			// 
			// NumericStartingLevel
			// 
			this.NumericStartingLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.NumericStartingLevel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NumericStartingLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.NumericStartingLevel.Location = new System.Drawing.Point(107, 118);
			this.NumericStartingLevel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericStartingLevel.Name = "NumericStartingLevel";
			this.NumericStartingLevel.Size = new System.Drawing.Size(108, 23);
			this.NumericStartingLevel.TabIndex = 19;
			this.NumericStartingLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// TabControlMain
			// 
			this.TabControlMain.Controls.Add(this.TabPageMobMaker);
			this.TabControlMain.Controls.Add(this.TabPageRoomEditor);
			this.TabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControlMain.Location = new System.Drawing.Point(0, 0);
			this.TabControlMain.Name = "TabControlMain";
			this.TabControlMain.SelectedIndex = 0;
			this.TabControlMain.Size = new System.Drawing.Size(1000, 800);
			this.TabControlMain.TabIndex = 21;
			// 
			// TabPageMobMaker
			// 
			this.TabPageMobMaker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
			this.TabPageMobMaker.Controls.Add(this.StartingLevel);
			this.TabPageMobMaker.Controls.Add(this.NumericStartingLevel);
			this.TabPageMobMaker.Controls.Add(this.label7);
			this.TabPageMobMaker.Controls.Add(this.label6);
			this.TabPageMobMaker.Controls.Add(this.label2);
			this.TabPageMobMaker.Controls.Add(this.CheckBoxHumanoid);
			this.TabPageMobMaker.Controls.Add(this.ComboFaction);
			this.TabPageMobMaker.Controls.Add(this.ComboDisposition);
			this.TabPageMobMaker.Controls.Add(this.label5);
			this.TabPageMobMaker.Controls.Add(this.label4);
			this.TabPageMobMaker.Controls.Add(this.label3);
			this.TabPageMobMaker.Controls.Add(this.Health);
			this.TabPageMobMaker.Controls.Add(this.NumericInt);
			this.TabPageMobMaker.Controls.Add(this.NumericDex);
			this.TabPageMobMaker.Controls.Add(this.NumericStr);
			this.TabPageMobMaker.Controls.Add(this.NumericHealth);
			this.TabPageMobMaker.Controls.Add(this.label1);
			this.TabPageMobMaker.Controls.Add(this.ComboMobList);
			this.TabPageMobMaker.Controls.Add(this.LabelName);
			this.TabPageMobMaker.Controls.Add(this.ButtonSubmit);
			this.TabPageMobMaker.Controls.Add(this.TextName);
			this.TabPageMobMaker.Location = new System.Drawing.Point(4, 26);
			this.TabPageMobMaker.Name = "TabPageMobMaker";
			this.TabPageMobMaker.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageMobMaker.Size = new System.Drawing.Size(992, 770);
			this.TabPageMobMaker.TabIndex = 0;
			this.TabPageMobMaker.Text = "Mob Maker";
			// 
			// TabPageRoomEditor
			// 
			this.TabPageRoomEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
			this.TabPageRoomEditor.Controls.Add(this.LabelRoomName);
			this.TabPageRoomEditor.Controls.Add(this.LabelRoomDesc);
			this.TabPageRoomEditor.Controls.Add(this.LabelRoomSpawners);
			this.TabPageRoomEditor.Controls.Add(this.LabelRoomExits);
			this.TabPageRoomEditor.Controls.Add(this.PanelExits);
			this.TabPageRoomEditor.Controls.Add(this.ButtonAddExit);
			this.TabPageRoomEditor.Controls.Add(this.ButtonAddNewRoom);
			this.TabPageRoomEditor.Controls.Add(this.ComboMapFiles);
			this.TabPageRoomEditor.Controls.Add(this.ListRooms);
			this.TabPageRoomEditor.Controls.Add(this.TextRoomName);
			this.TabPageRoomEditor.Controls.Add(this.TextRoomDesc);
			this.TabPageRoomEditor.Controls.Add(this.TextRoomSpawners);
			this.TabPageRoomEditor.Controls.Add(this.RichTextAnsiMap);
			this.TabPageRoomEditor.Controls.Add(this.ButtonNorth);
			this.TabPageRoomEditor.Controls.Add(this.ButtonSouth);
			this.TabPageRoomEditor.Controls.Add(this.ButtonEast);
			this.TabPageRoomEditor.Controls.Add(this.ButtonWest);
			this.TabPageRoomEditor.Controls.Add(this.ButtonUp);
			this.TabPageRoomEditor.Controls.Add(this.ButtonDown);
			this.TabPageRoomEditor.Controls.Add(this.ButtonSaveAll);
			this.TabPageRoomEditor.Location = new System.Drawing.Point(4, 26);
			this.TabPageRoomEditor.Name = "TabPageRoomEditor";
			this.TabPageRoomEditor.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageRoomEditor.Size = new System.Drawing.Size(992, 770);
			this.TabPageRoomEditor.TabIndex = 1;
			this.TabPageRoomEditor.Text = "Room Editor";
			// 
			// ComboMapFiles
			// 
			this.ComboMapFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ComboMapFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboMapFiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ComboMapFiles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ComboMapFiles.FormattingEnabled = true;
			this.ComboMapFiles.Location = new System.Drawing.Point(8, 6);
			this.ComboMapFiles.Name = "ComboMapFiles";
			this.ComboMapFiles.Size = new System.Drawing.Size(250, 23);
			this.ComboMapFiles.TabIndex = 0;
			this.ComboMapFiles.SelectedIndexChanged += new System.EventHandler(this.ComboMapFiles_SelectedIndexChanged);
			// 
			// ListRooms
			// 
			this.ListRooms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ListRooms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ListRooms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this.ListRooms.ContextMenuStrip = this.ContextMenuRooms;
			this.ListRooms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ListRooms.FullRowSelect = true;
			this.ListRooms.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.ListRooms.Location = new System.Drawing.Point(8, 37);
			this.ListRooms.MultiSelect = false;
			this.ListRooms.Name = "ListRooms";
			this.ListRooms.Size = new System.Drawing.Size(250, 707);
			this.ListRooms.TabIndex = 1;
			this.ListRooms.UseCompatibleStateImageBehavior = false;
			this.ListRooms.View = System.Windows.Forms.View.Details;
			this.ListRooms.SelectedIndexChanged += new System.EventHandler(this.ListRooms_SelectedIndexChanged);
			this.ListRooms.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListRooms_MouseDown);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Room Name";
			this.columnHeader1.Width = 160;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Coordinates";
			this.columnHeader2.Width = 85;
			// 
			// ContextMenuRooms
			// 
			this.ContextMenuRooms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.MenuItemDeleteRoom});
			this.ContextMenuRooms.Name = "ContextMenuRooms";
			this.ContextMenuRooms.Size = new System.Drawing.Size(143, 26);
			// 
			// MenuItemDeleteRoom
			// 
			this.MenuItemDeleteRoom.Name = "MenuItemDeleteRoom";
			this.MenuItemDeleteRoom.Size = new System.Drawing.Size(142, 22);
			this.MenuItemDeleteRoom.Text = "Delete Room";
			this.MenuItemDeleteRoom.Click += new System.EventHandler(this.MenuItemDeleteRoom_Click);
			// 
			// LabelRoomName
			// 
			this.LabelRoomName.AutoSize = true;
			this.LabelRoomName.Location = new System.Drawing.Point(266, 17);
			this.LabelRoomName.Name = "LabelRoomName";
			this.LabelRoomName.Size = new System.Drawing.Size(76, 17);
			this.LabelRoomName.Text = "Room Name";
			// 
			// TextRoomName
			// 
			this.TextRoomName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.TextRoomName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextRoomName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.TextRoomName.Location = new System.Drawing.Point(266, 37);
			this.TextRoomName.Name = "TextRoomName";
			this.TextRoomName.Size = new System.Drawing.Size(350, 23);
			this.TextRoomName.TabIndex = 2;
			// 
			// LabelRoomDesc
			// 
			this.LabelRoomDesc.AutoSize = true;
			this.LabelRoomDesc.Location = new System.Drawing.Point(266, 67);
			this.LabelRoomDesc.Name = "LabelRoomDesc";
			this.LabelRoomDesc.Size = new System.Drawing.Size(72, 17);
			this.LabelRoomDesc.Text = "Description";
			// 
			// TextRoomDesc
			// 
			this.TextRoomDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.TextRoomDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextRoomDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.TextRoomDesc.Location = new System.Drawing.Point(266, 87);
			this.TextRoomDesc.Multiline = true;
			this.TextRoomDesc.Name = "TextRoomDesc";
			this.TextRoomDesc.Size = new System.Drawing.Size(350, 100);
			this.TextRoomDesc.TabIndex = 3;
			// 
			// LabelRoomSpawners
			// 
			this.LabelRoomSpawners.AutoSize = true;
			this.LabelRoomSpawners.Location = new System.Drawing.Point(266, 193);
			this.LabelRoomSpawners.Name = "LabelRoomSpawners";
			this.LabelRoomSpawners.Size = new System.Drawing.Size(61, 17);
			this.LabelRoomSpawners.Text = "Spawners";
			// 
			// TextRoomSpawners
			// 
			this.TextRoomSpawners.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.TextRoomSpawners.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextRoomSpawners.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.TextRoomSpawners.Location = new System.Drawing.Point(266, 213);
			this.TextRoomSpawners.Multiline = true;
			this.TextRoomSpawners.Name = "TextRoomSpawners";
			this.TextRoomSpawners.Size = new System.Drawing.Size(350, 100);
			this.TextRoomSpawners.TabIndex = 4;
			// 
			// LabelRoomExits
			// 
			this.LabelRoomExits.AutoSize = true;
			this.LabelRoomExits.Location = new System.Drawing.Point(266, 319);
			this.LabelRoomExits.Name = "LabelRoomExits";
			this.LabelRoomExits.Size = new System.Drawing.Size(35, 17);
			this.LabelRoomExits.Text = "Exits";
			// 
			// PanelExits
			// 
			this.PanelExits.AutoScroll = true;
			this.PanelExits.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.PanelExits.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PanelExits.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.PanelExits.Location = new System.Drawing.Point(266, 339);
			this.PanelExits.Name = "PanelExits";
			this.PanelExits.Size = new System.Drawing.Size(350, 200);
			this.PanelExits.TabIndex = 5;
			this.PanelExits.WrapContents = false;
			// 
			// ButtonAddExit
			// 
			this.ButtonAddExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonAddExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonAddExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonAddExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonAddExit.Location = new System.Drawing.Point(266, 545);
			this.ButtonAddExit.Name = "ButtonAddExit";
			this.ButtonAddExit.Size = new System.Drawing.Size(100, 30);
			this.ButtonAddExit.TabIndex = 14;
			this.ButtonAddExit.Text = "Add Exit";
			this.ButtonAddExit.UseVisualStyleBackColor = false;
			this.ButtonAddExit.Click += new System.EventHandler(this.ButtonAddExit_Click);
			// 
			// ButtonAddNewRoom
			// 
			this.ButtonAddNewRoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonAddNewRoom.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonAddNewRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonAddNewRoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonAddNewRoom.Location = new System.Drawing.Point(372, 545);
			this.ButtonAddNewRoom.Name = "ButtonAddNewRoom";
			this.ButtonAddNewRoom.Size = new System.Drawing.Size(100, 30);
			this.ButtonAddNewRoom.TabIndex = 15;
			this.ButtonAddNewRoom.Text = "Add Room";
			this.ButtonAddNewRoom.UseVisualStyleBackColor = false;
			this.ButtonAddNewRoom.Click += new System.EventHandler(this.ButtonAddNewRoom_Click);
			// 
			// RichTextAnsiMap
			// 
			this.RichTextAnsiMap.BackColor = System.Drawing.Color.Black;
			this.RichTextAnsiMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RichTextAnsiMap.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RichTextAnsiMap.ForeColor = System.Drawing.Color.White;
			this.RichTextAnsiMap.Location = new System.Drawing.Point(624, 37);
			this.RichTextAnsiMap.Name = "RichTextAnsiMap";
			this.RichTextAnsiMap.ReadOnly = true;
			this.RichTextAnsiMap.Size = new System.Drawing.Size(360, 464);
			this.RichTextAnsiMap.TabIndex = 6;
			this.RichTextAnsiMap.Text = "";
			// 
			// ButtonNorth
			// 
			this.ButtonNorth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonNorth.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonNorth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonNorth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonNorth.Location = new System.Drawing.Point(726, 507);
			this.ButtonNorth.Name = "ButtonNorth";
			this.ButtonNorth.Size = new System.Drawing.Size(75, 23);
			this.ButtonNorth.TabIndex = 7;
			this.ButtonNorth.Text = "North";
			this.ButtonNorth.UseVisualStyleBackColor = false;
			this.ButtonNorth.Click += new System.EventHandler(this.ButtonNorth_Click);
			// 
			// ButtonSouth
			// 
			this.ButtonSouth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonSouth.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonSouth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonSouth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonSouth.Location = new System.Drawing.Point(726, 565);
			this.ButtonSouth.Name = "ButtonSouth";
			this.ButtonSouth.Size = new System.Drawing.Size(75, 23);
			this.ButtonSouth.TabIndex = 8;
			this.ButtonSouth.Text = "South";
			this.ButtonSouth.UseVisualStyleBackColor = false;
			this.ButtonSouth.Click += new System.EventHandler(this.ButtonSouth_Click);
			// 
			// ButtonEast
			// 
			this.ButtonEast.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonEast.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonEast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonEast.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonEast.Location = new System.Drawing.Point(807, 536);
			this.ButtonEast.Name = "ButtonEast";
			this.ButtonEast.Size = new System.Drawing.Size(75, 23);
			this.ButtonEast.TabIndex = 9;
			this.ButtonEast.Text = "East";
			this.ButtonEast.UseVisualStyleBackColor = false;
			this.ButtonEast.Click += new System.EventHandler(this.ButtonEast_Click);
			// 
			// ButtonWest
			// 
			this.ButtonWest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonWest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonWest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonWest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonWest.Location = new System.Drawing.Point(645, 536);
			this.ButtonWest.Name = "ButtonWest";
			this.ButtonWest.Size = new System.Drawing.Size(75, 23);
			this.ButtonWest.TabIndex = 10;
			this.ButtonWest.Text = "West";
			this.ButtonWest.UseVisualStyleBackColor = false;
			this.ButtonWest.Click += new System.EventHandler(this.ButtonWest_Click);
			// 
			// ButtonUp
			// 
			this.ButtonUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonUp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonUp.Location = new System.Drawing.Point(888, 507);
			this.ButtonUp.Name = "ButtonUp";
			this.ButtonUp.Size = new System.Drawing.Size(75, 23);
			this.ButtonUp.TabIndex = 11;
			this.ButtonUp.Text = "Up";
			this.ButtonUp.UseVisualStyleBackColor = false;
			this.ButtonUp.Click += new System.EventHandler(this.ButtonUp_Click);
			// 
			// ButtonDown
			// 
			this.ButtonDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonDown.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonDown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonDown.Location = new System.Drawing.Point(888, 565);
			this.ButtonDown.Name = "ButtonDown";
			this.ButtonDown.Size = new System.Drawing.Size(75, 23);
			this.ButtonDown.TabIndex = 12;
			this.ButtonDown.Text = "Down";
			this.ButtonDown.UseVisualStyleBackColor = false;
			this.ButtonDown.Click += new System.EventHandler(this.ButtonDown_Click);
			// 
			// ButtonSaveAll
			// 
			this.ButtonSaveAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
			this.ButtonSaveAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
			this.ButtonSaveAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonSaveAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ButtonSaveAll.Location = new System.Drawing.Point(266, 600);
			this.ButtonSaveAll.Name = "ButtonSaveAll";
			this.ButtonSaveAll.Size = new System.Drawing.Size(350, 30);
			this.ButtonSaveAll.TabIndex = 13;
			this.ButtonSaveAll.Text = "Save All";
			this.ButtonSaveAll.UseVisualStyleBackColor = false;
			this.ButtonSaveAll.Click += new System.EventHandler(this.ButtonSaveAll_Click);
			// 
			// ContentCreator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
			this.ClientSize = new System.Drawing.Size(1000, 800);
			this.Controls.Add(this.TabControlMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "ContentCreator";
			this.Text = "Content Creator";
			((System.ComponentModel.ISupportInitialize)(this.NumericHealth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericStr)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericDex)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericInt)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spawnDataBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NumericStartingLevel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TextName;
		private System.Windows.Forms.Button ButtonSubmit;
		private System.Windows.Forms.Label LabelName;
		private System.Windows.Forms.ComboBox ComboMobList;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown NumericHealth;
		private System.Windows.Forms.NumericUpDown NumericStr;
		private System.Windows.Forms.NumericUpDown NumericDex;
		private System.Windows.Forms.NumericUpDown NumericInt;
		private System.Windows.Forms.Label Health;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox ComboDisposition;
		private System.Windows.Forms.ComboBox ComboFaction;
		private System.Windows.Forms.CheckBox CheckBoxHumanoid;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label StartingLevel;
		private System.Windows.Forms.NumericUpDown NumericStartingLevel;
		private System.Windows.Forms.BindingSource spawnDataBindingSource;
		private System.Windows.Forms.TabControl TabControlMain;
		private System.Windows.Forms.TabPage TabPageMobMaker;
		private System.Windows.Forms.TabPage TabPageRoomEditor;
		private System.Windows.Forms.ComboBox ComboMapFiles;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ListView ListRooms;
		private System.Windows.Forms.TextBox TextRoomName;
		private System.Windows.Forms.TextBox TextRoomDesc;
		private System.Windows.Forms.TextBox TextRoomSpawners;
		private System.Windows.Forms.RichTextBox RichTextAnsiMap;
		private System.Windows.Forms.Label LabelRoomName;
		private System.Windows.Forms.Label LabelRoomDesc;
		private System.Windows.Forms.Label LabelRoomSpawners;
		private System.Windows.Forms.Label LabelRoomExits;
		private System.Windows.Forms.FlowLayoutPanel PanelExits;
		private System.Windows.Forms.Button ButtonAddExit;
		private System.Windows.Forms.Button ButtonAddNewRoom;
		private System.Windows.Forms.Button ButtonNorth;
		private System.Windows.Forms.Button ButtonSouth;
		private System.Windows.Forms.Button ButtonEast;
		private System.Windows.Forms.Button ButtonWest;
		private System.Windows.Forms.Button ButtonUp;
		private System.Windows.Forms.Button ButtonDown;
		private System.Windows.Forms.Button ButtonSaveAll;
		private System.Windows.Forms.ContextMenuStrip ContextMenuRooms;
		private System.Windows.Forms.ToolStripMenuItem MenuItemDeleteRoom;
	}
}
namespace ServerCore {
	partial class MobMaker {
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
			this.TextName.Location = new System.Drawing.Point(110, 53);
			this.TextName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.TextName.Name = "TextName";
			this.TextName.Size = new System.Drawing.Size(261, 24);
			this.TextName.TabIndex = 0;
			// 
			// ButtonSubmit
			// 
			this.ButtonSubmit.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.ButtonSubmit.Location = new System.Drawing.Point(14, 16);
			this.ButtonSubmit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ButtonSubmit.Name = "ButtonSubmit";
			this.ButtonSubmit.Size = new System.Drawing.Size(87, 30);
			this.ButtonSubmit.TabIndex = 1;
			this.ButtonSubmit.Text = "Submit";
			this.ButtonSubmit.UseVisualStyleBackColor = true;
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
			this.ComboMobList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboMobList.Font = new System.Drawing.Font("Yu Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
			this.NumericHealth.Location = new System.Drawing.Point(107, 151);
			this.NumericHealth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericHealth.Name = "NumericHealth";
			this.NumericHealth.Size = new System.Drawing.Size(108, 24);
			this.NumericHealth.TabIndex = 5;
			this.NumericHealth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericStr
			// 
			this.NumericStr.Location = new System.Drawing.Point(107, 185);
			this.NumericStr.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericStr.Name = "NumericStr";
			this.NumericStr.Size = new System.Drawing.Size(108, 24);
			this.NumericStr.TabIndex = 6;
			this.NumericStr.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericDex
			// 
			this.NumericDex.Location = new System.Drawing.Point(107, 219);
			this.NumericDex.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericDex.Name = "NumericDex";
			this.NumericDex.Size = new System.Drawing.Size(108, 24);
			this.NumericDex.TabIndex = 7;
			this.NumericDex.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// NumericInt
			// 
			this.NumericInt.Location = new System.Drawing.Point(107, 253);
			this.NumericInt.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericInt.Name = "NumericInt";
			this.NumericInt.Size = new System.Drawing.Size(108, 24);
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
			this.ComboDisposition.DataSource = this.spawnDataBindingSource;
			this.ComboDisposition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboDisposition.FormattingEnabled = true;
			this.ComboDisposition.Location = new System.Drawing.Point(107, 288);
			this.ComboDisposition.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComboDisposition.Name = "ComboDisposition";
			this.ComboDisposition.Size = new System.Drawing.Size(108, 25);
			this.ComboDisposition.TabIndex = 13;
			// 
			// spawnDataBindingSource
			// 
			this.spawnDataBindingSource.DataSource = typeof(GameCore.Util.SpawnData);
			// 
			// ComboFaction
			// 
			this.ComboFaction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboFaction.FormattingEnabled = true;
			this.ComboFaction.Location = new System.Drawing.Point(107, 324);
			this.ComboFaction.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.ComboFaction.Name = "ComboFaction";
			this.ComboFaction.Size = new System.Drawing.Size(108, 25);
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
			this.NumericStartingLevel.Location = new System.Drawing.Point(107, 118);
			this.NumericStartingLevel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.NumericStartingLevel.Name = "NumericStartingLevel";
			this.NumericStartingLevel.Size = new System.Drawing.Size(108, 24);
			this.NumericStartingLevel.TabIndex = 19;
			this.NumericStartingLevel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// MobMaker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(385, 363);
			this.Controls.Add(this.StartingLevel);
			this.Controls.Add(this.NumericStartingLevel);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.CheckBoxHumanoid);
			this.Controls.Add(this.ComboFaction);
			this.Controls.Add(this.ComboDisposition);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.Health);
			this.Controls.Add(this.NumericInt);
			this.Controls.Add(this.NumericDex);
			this.Controls.Add(this.NumericStr);
			this.Controls.Add(this.NumericHealth);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ComboMobList);
			this.Controls.Add(this.LabelName);
			this.Controls.Add(this.ButtonSubmit);
			this.Controls.Add(this.TextName);
			this.Font = new System.Drawing.Font("Palatino Linotype", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.SystemColors.ButtonFace;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.Name = "MobMaker";
			this.Text = "Mob Maker";
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
	}
}
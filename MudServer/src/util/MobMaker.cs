using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameCore;
using GameCore.Util;



namespace ServerCore {

	public partial class MobMaker : Form {

		SpawnData ActiveTemplate;

		public MobMaker() {

			InitializeComponent();
			RefreshMobList();
			PopulateDropDownLists();
		}

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
				if (Data.NameSpawnPairs.ContainsKey(TextName.Text)) {
					DialogResult result = MessageBox.Show("This entry exists. Overwite?.",
					"Warning!",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1);

					switch (result) {
						case DialogResult.Yes:
							ActiveTemplate = Data.NameSpawnPairs[TextName.Text];
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
				Data.NameSpawnPairs.Add(ActiveTemplate.Name, ActiveTemplate);
			} catch (ArgumentException) {
				Data.NameSpawnPairs[TextName.Text] = ActiveTemplate;
			}

			Data.SaveSpawnTemplates();
			RefreshMobList();
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
			ActiveTemplate.Faction = (Group)Enum.Parse(typeof(Group), ComboFaction.SelectedItem.ToString());
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
			ComboFaction.SelectedItem = Enum.GetName(typeof(Group), ActiveTemplate.Faction);

		}

		void RefreshMobList() {

			ComboMobList.Items.Clear();
			foreach (var entry in Data.NameSpawnPairs) {
				ComboMobList.Items.Add(entry.Key);
			}
		}

		private void ComboMobList_SelectionChangeCommitted(object sender, EventArgs e) {

			ActiveTemplate = Data.NameSpawnPairs[(string)ComboMobList.SelectedItem];
			SetTemplateValuesToForm();

		}

		private void PopulateDropDownLists() {

			ComboDisposition.DataSource = Enum.GetNames(typeof(Disposition));
			ComboFaction.DataSource = Enum.GetNames(typeof(Group));
		}
	}
}

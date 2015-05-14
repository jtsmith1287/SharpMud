using System;

namespace MudServer {
	public partial class MobMaker : Gtk.Window {
		public MobMaker () : 
				base(Gtk.WindowType.Toplevel) {
			this.Build ();
		}
	}
}


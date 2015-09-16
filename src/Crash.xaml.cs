using System;
using System.Windows;
using System.Diagnostics;

namespace Rooler {
	/// <summary>
	/// Interaction logic for Crash.xaml
	/// </summary>
	public partial class Crash : Window {
		public Crash(string text) {
			InitializeComponent();

			this.TB.Text = text;

			this.Hyperlink.NavigateUri = new Uri(this.Hyperlink.NavigateUri.OriginalString + "&body=(copy_error_info_here)");

			this.Hyperlink.Click += delegate {
				Process.Start('"' + this.Hyperlink.NavigateUri.OriginalString + '"');
			};
		}
	}
}

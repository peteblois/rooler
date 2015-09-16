using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Rooler {

	public partial class App : Application {


		public App() {

			this.DispatcherUnhandledException += App.UnhandledExceptionHandler;

			this.InitializeComponent();

			//this.mainWindow = new MainWindow();
			//this.mainWindow.Show();

			//this.Run();
		}

		private static void UnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e) {
			e.Handled = true;

			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(delegate {

				Crash crash = new Crash(e.Exception.Message + e.Exception.StackTrace);
				crash.Show();
				return null;
			}), null);
		}
	}
}

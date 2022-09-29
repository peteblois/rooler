using System;
using System.Windows;

namespace Rooler
{
	public interface IScreenService
	{
		FrameworkElement Visual { get; }

		void CloseService();
		void Update();

		event EventHandler ServiceClosed;

		bool IsFrozen { get; }
	}

	public interface IScreenServiceHost
	{
		bool PreserveAnnotations { get; }
		IScreenShot CurrentScreen { get; }
	}
}

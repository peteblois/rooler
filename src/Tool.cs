using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rooler
{
	public class Tool : ContentControl, IScreenService
	{

		public Tool(IScreenServiceHost host)
		{
			this.Host = host;
			this.Focusable = true;

			this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
			this.VerticalContentAlignment = VerticalAlignment.Stretch;

			this.Loaded += delegate
			{
				this.Focus();
			};

		}

		protected IScreenServiceHost Host { get; set; }

		public FrameworkElement Visual
		{
			get { return this; }
		}

		public event EventHandler ServiceClosed;
		public void CloseService()
		{
			this.OnClosing();

			if (this.ServiceClosed != null)
				this.ServiceClosed(this, EventArgs.Empty);
		}

		public virtual void Update()
		{

		}


		private bool isFrozen = false;
		public bool IsFrozen
		{
			get { return this.isFrozen; }
			private set { this.isFrozen = value; }
		}

		protected virtual void Freeze()
		{
			this.IsFrozen = true;
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					{
						IntPoint cursorPos = NativeMethods.GetCursorPos();
						cursorPos.X -= 1;
						NativeMethods.SetCursorPos(cursorPos);
					}
					e.Handled = true;
					break;
				case Key.Right:
					{
						IntPoint cursorPos = NativeMethods.GetCursorPos();
						cursorPos.X += 1;
						NativeMethods.SetCursorPos(cursorPos);
					}
					e.Handled = true;
					break;
				case Key.Up:
					{
						IntPoint cursorPos = NativeMethods.GetCursorPos();
						cursorPos.Y -= 1;
						NativeMethods.SetCursorPos(cursorPos);
					}
					e.Handled = true;
					break;
				case Key.Down:
					{
						IntPoint cursorPos = NativeMethods.GetCursorPos();
						cursorPos.Y += 1;
						NativeMethods.SetCursorPos(cursorPos);
					}
					e.Handled = true;
					break;
			}
			base.OnKeyDown(e);
		}

		protected virtual void OnClosing()
		{
		}
	}
}

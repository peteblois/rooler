using System;
using System.Windows;
using System.Windows.Input;

namespace Rooler
{
	public enum StretchMode
	{
		NorthSouth,
		EastWest,
		NorthSouthEastWest,
	}

	/// <summary>
	/// Interaction logic for DistanceAdorner.xaml
	/// </summary>
	public partial class DistanceTool : Tool, IScreenService
	{

		private IntPoint previousPoint = new IntPoint();
		private IScreenShot screenshot;

		public DistanceTool(StretchMode stretch, IScreenServiceHost host) : base(host)
		{
			this.StretchMode = stretch;

			this.screenshot = this.Host.CurrentScreen;

			this.InitializeComponent();

			if (this.StretchMode == StretchMode.EastWest)
			{
				this.N.Visibility = Visibility.Collapsed;
				this.S.Visibility = Visibility.Collapsed;
			}
			else if (this.StretchMode == StretchMode.NorthSouth)
			{
				this.E.Visibility = Visibility.Collapsed;
				this.W.Visibility = Visibility.Collapsed;
			}
#if !DEBUG
			this.Cursor = Cursors.None;
#endif

			this.Dimensions.CloseClicked += delegate
			{
				this.CloseService();
			};

			this.Loaded += delegate
			{
				this.Update();
			};
		}


		public StretchMode StretchMode { get; set; }

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!this.IsFrozen)
				this.Update(false);
		}

		public override void Update()
		{
			this.Update(true);
		}

		private void Update(bool force)
		{
			IntPoint mousePt = NativeMethods.GetCursorPos();

			if (force || mousePt != this.previousPoint)
			{
				this.previousPoint = mousePt;

				IntRect rect = ScreenCoordinates.ExpandPoint(mousePt, this.screenshot);
				if (!rect.IsEmpty)
				{
					this.UpdateBounds(rect, mousePt);
				}
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (!this.Host.PreserveAnnotations)
				this.CloseService();
			else
				this.Freeze();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					this.Freeze();
					e.Handled = true;
					break;
			}
			base.OnKeyDown(e);
		}

		protected override void Freeze()
		{
			base.Freeze();

			this.Background = null;
			this.Cursor = Cursors.Arrow;
			this.Dimensions.CanClose = true;
		}

		protected void UpdateBounds(IntRect screenBounds, IntPoint point)
		{

			Rect bounds = NativeMethods.ScreenToClient(this, screenBounds);
			Point screenPoint = NativeMethods.ScreenToClient(this, point);

			this.Bounds.Width = bounds.Width;
			this.Bounds.Height = bounds.Height;


			this.Bounds.Margin = new Thickness(bounds.Left, bounds.Top, 0, 0);

			this.CenterY.Height = new GridLength(Math.Max(screenPoint.Y - bounds.Y, 0));
			this.CenterX.Width = new GridLength(Math.Max(screenPoint.X - bounds.X, 0));


			var widthWpf = bounds.Width;
			var heightWpf = bounds.Height;
			var widthNative = screenBounds.Width;
			var heightNative = screenBounds.Height;

			if (this.StretchMode == StretchMode.EastWest)
			{
				if (!ScreenShot.HasDisplayScaling)
				{
					this.Dimensions.Text = $@"{(int)widthNative}";
				}
				else
				{
					this.Dimensions.Text = $@"{(int)widthWpf} ({(int)widthNative})";
				}
			}
			else if (this.StretchMode == StretchMode.NorthSouth)
			{
				if (!ScreenShot.HasDisplayScaling)
				{
					this.Dimensions.Text = $@"{(int)heightNative}";
				}
				else
				{
					this.Dimensions.Text = $@"{(int)heightWpf} ({(int)heightNative})";
				}
			}
			else
			{
				if (!ScreenShot.HasDisplayScaling)
				{
					this.Dimensions.Text = $@"{(int)widthNative} x {(int)heightNative}";
				}
				else
				{
					this.Dimensions.Text = $@"{(int)widthWpf} x {(int)heightWpf} ({(int)widthNative} x {(int)heightNative})";
				}
			}
		}
	}
}

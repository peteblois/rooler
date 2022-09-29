using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Rooler {
	/// <summary>
	/// Interaction logic for BoundsFinder.xaml
	/// </summary>
	public partial class BoundsTool : Tool {

		private IntPoint mouseDownLoc = new IntPoint();
		private Rect bounds;
		private IntRect screenBounds;
		private IScreenShot screenshot;

		public BoundsTool(IScreenServiceHost host): base(host) {
			this.screenshot = this.Host.CurrentScreen;

			InitializeComponent();

			this.BoundsWidth.Width = new GridLength(0, GridUnitType.Pixel);
			this.BoundsHeight.Height = new GridLength(0, GridUnitType.Pixel);
			this.Dimensions.Visibility = Visibility.Collapsed;

			this.Dimensions.CloseClicked += delegate {
				this.CloseService();
			};
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {

			if (this.CaptureMouse()) {

				this.mouseDownLoc = NativeMethods.GetCursorPos();

				this.Bounds = new IntRect(this.mouseDownLoc.X, this.mouseDownLoc.Y, 0, 0);

				this.Cursor = Cursors.None;

				e.Handled = true;

			}
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonUp(e);

			if (this.IsMouseCaptured) {
				this.ReleaseMouseCapture();
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			switch (e.Key) {
				case Key.Enter:
					this.Freeze();
					e.Handled = true;
					break;
			}

			base.OnKeyDown(e);
		}

		protected override void Freeze() {
			base.Freeze();

			this.Cursor = Cursors.Arrow;
			this.Dimensions.CanClose = true;
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);

			if (this.IsMouseCaptured) {
				IntPoint mousePosition = NativeMethods.GetCursorPos();

				IntPoint topLeft = new IntPoint(Math.Min(mousePosition.X, this.mouseDownLoc.X), Math.Min(mousePosition.Y, this.mouseDownLoc.Y));
				IntPoint bottomRight = new IntPoint(Math.Max(mousePosition.X, this.mouseDownLoc.X), Math.Max(mousePosition.Y, this.mouseDownLoc.Y));

				this.Bounds = new IntRect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
			}
		}

		private bool IsGrayed {
			get {
				return ((SolidColorBrush)this.Resources["MaskBackground"]).Color.A != 0;
			}
			set {
				Color currentColor = ((SolidColorBrush)this.Resources["MaskBackground"]).Color;
				if (value)
					((SolidColorBrush)this.Resources["MaskBackground"]).Color = Color.FromArgb(0x40, currentColor.R, currentColor.G, currentColor.B);
				else
					((SolidColorBrush)this.Resources["MaskBackground"]).Color = Color.FromArgb(0x00, currentColor.R, currentColor.G, currentColor.B);
			}
		}

		private IntRect Bounds {
			get { return this.screenBounds; }
			set {
				this.screenBounds = value;

				this.bounds = NativeMethods.ScreenToClient(this, this.screenBounds);

				this.BoundsOffsetX.Width = new GridLength(this.bounds.Left, GridUnitType.Pixel);
				this.BoundsOffsetY.Height = new GridLength(this.bounds.Top, GridUnitType.Pixel);

				this.BoundsWidth.Width = new GridLength(this.bounds.Width, GridUnitType.Pixel);
				this.BoundsHeight.Height = new GridLength(this.bounds.Height, GridUnitType.Pixel);

				this.Dimensions.Visibility = Visibility.Visible;
				this.Dimensions.Text = string.Format(@"{0} x {1}", this.screenBounds.Width, this.screenBounds.Height);
			}
		}

		private void AnimateBounds(IntRect screenBounds) {

			this.screenBounds = screenBounds;
			this.bounds = NativeMethods.ScreenToClient(this, this.screenBounds);

			TimeSpan duration = TimeSpan.FromSeconds(.2);
			this.BoundsOffsetX.AnimateTo(this.bounds.Left, duration);
			this.BoundsOffsetY.AnimateTo(this.bounds.Top, duration);

			this.BoundsWidth.AnimateTo(this.bounds.Width, duration);
			this.BoundsHeight.AnimateTo(this.bounds.Height, duration);

			this.Dimensions.Visibility = Visibility.Visible;
			this.Dimensions.Text = string.Format(@"{0} x {1}", this.screenBounds.Width, this.screenBounds.Height);
		}



		protected override void  OnLostMouseCapture(MouseEventArgs e) {
			base.OnLostMouseCapture(e);

			if (this.bounds.Width == 0 || this.bounds.Height == 0) {
				this.CloseService();

				return;
			}

			this.IsGrayed = false;
			this.Cursor = Cursors.Arrow;

			IntRect startRect = NativeMethods.ClientToScreen(this, this.bounds);
			startRect.Width -= 1;
			startRect.Height -= 1;
			IntRect screenBounds = ScreenCoordinates.Collapse(startRect, this.screenshot);

			if (!screenBounds.IsEmpty)
				this.AnimateBounds(screenBounds);

			this.BoundsRect.Visibility = Visibility.Visible;
			this.Dimensions.CanClose = true;
		}
	}
}

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Rooler {

	public enum StretchMode {
		NorthSouth,
		EastWest,
		NorthSouthEastWest,
	}
	/// <summary>
	/// Interaction logic for DistanceAdorner.xaml
	/// </summary>
	public partial class DistanceTool : Tool, IScreenService {

		private IntPoint previousPoint = new IntPoint();
		private IScreenShot screenshot;

		public DistanceTool(StretchMode stretch, IScreenServiceHost host): base(host) {
			this.StretchMode = stretch;

			this.screenshot = this.Host.CurrentScreen;

			this.InitializeComponent();

			if (this.StretchMode == StretchMode.EastWest) {
				this.N.Visibility = Visibility.Collapsed;
				this.S.Visibility = Visibility.Collapsed;
			}
			else if (this.StretchMode == StretchMode.NorthSouth) {
				this.E.Visibility = Visibility.Collapsed;
				this.W.Visibility = Visibility.Collapsed;
			}
#if !DEBUG
			this.Cursor = Cursors.None;
#endif

			this.Dimensions.CloseClicked += delegate {
				this.CloseService();
			};

			this.Loaded += delegate {
				this.Update();
			};
		}
		

		public StretchMode StretchMode { get; set; }

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);

			if (!this.IsFrozen)
				this.Update(false);
		}

		public override void Update() {
			this.Update(true);
		}

		private void Update(bool force) {
			IntPoint mousePt = NativeMethods.GetCursorPos();

			if (Keyboard.IsKeyDown(Key.Space)) {
			      }

			if (force || mousePt != this.previousPoint) {
				this.previousPoint = mousePt;

				IntRect rect = ScreenCoordinates.ExpandPoint(mousePt, this.screenshot);
				if (!rect.IsEmpty) {
					this.UpdateBounds(rect, mousePt);
				}
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);

			if (!this.Host.PreserveAnnotations)
				this.CloseService();
			else
				this.Freeze();
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

			this.Background = null;
			this.Cursor = Cursors.Arrow;
			this.Dimensions.CanClose = true;
		}

		protected void UpdateBounds(IntRect screenBounds, IntPoint point) {

			Rect bounds = NativeMethods.ScreenToClient(this, screenBounds);
			Point screenPoint = NativeMethods.ScreenToClient(this, point);

			this.Bounds.Width = bounds.Width;
			this.Bounds.Height = bounds.Height;

			
			this.Bounds.Margin = new Thickness(bounds.Left, bounds.Top, 0, 0);

			this.CenterY.Height = new GridLength(Math.Max(screenPoint.Y - bounds.Y, 0));
			this.CenterX.Width = new GridLength(Math.Max(screenPoint.X - bounds.X, 0));

			if (this.StretchMode == StretchMode.EastWest)
				this.Dimensions.Text = string.Format(@"{0}", screenBounds.Width);
			else if (this.StretchMode == StretchMode.NorthSouth)
				this.Dimensions.Text = string.Format(@"{0}", screenBounds.Height);
			else
				this.Dimensions.Text = string.Format(@"{0} x {1}", screenBounds.Width, screenBounds.Height);
		}

		//protected override void OnKeyUp(KeyEventArgs e) {
		//    base.OnKeyUp(e);

		//    if (e.Key == Key.D) {
		//        VirtualizedScreenShot vs = this.screenshot as VirtualizedScreenShot;
		//        if (vs != null) {
		//            Point mousePt = NativeMethods.GetCursorPos();
		//            ScreenShot currentTile = vs.GetTile((int)mousePt.X, (int)mousePt.Y);

		//            if (currentTile != null) {
		//                BitmapFrame frame = BitmapFrame.Create(currentTile.Image);
		//                PngBitmapEncoder encoder = new PngBitmapEncoder();
		//                encoder.Frames.Add(frame);

		//                using (FileStream fs = new FileStream(@"C:\tmp\ss.png", FileMode.Create, FileAccess.Write)) {
		//                    encoder.Save(fs);
		//                };
		//            }
		//        }
		//    }
		//}
	}
}

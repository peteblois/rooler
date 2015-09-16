using System;
using System.Windows;
using System.Windows.Controls;

namespace Rooler {
	public static class Extensions {
		public static int Clamp(this int value, int min, int max) {
			return Math.Min(max, Math.Max(value, min));
		}

		public static double Clamp(this double value, double min, double max) {
			return Math.Min(max, Math.Max(value, min));
		}

		public static void AnimateTo(this ColumnDefinition column, double pixelValue, TimeSpan duration) {
			GridLengthAnimation widthAnim = new GridLengthAnimation() {
			    To = pixelValue,
			    Duration = new Duration(duration)
			};

			column.BeginAnimation(ColumnDefinition.WidthProperty, widthAnim);
		}

		public static void AnimateTo(this RowDefinition row, double pixelValue, TimeSpan duration) {
			GridLengthAnimation heightAnim = new GridLengthAnimation() {
				To = pixelValue,
				Duration = new Duration(duration)
			};

			row.BeginAnimation(RowDefinition.HeightProperty, heightAnim);
		}
	}
}

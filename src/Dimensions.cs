using System;
using System.Windows;
using System.Windows.Controls;

namespace Rooler {

	public class Dimensions : Control {
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(Dimensions), new PropertyMetadata(null));
		public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register("CanClose", typeof(bool), typeof(Dimensions), new PropertyMetadata(false));

		static Dimensions() {
			Dimensions.DefaultStyleKeyProperty.OverrideMetadata(typeof(Dimensions), new FrameworkPropertyMetadata(typeof(Dimensions)));
		}

		public event EventHandler CloseClicked;

		

		public string Text {
			get { return (string)this.GetValue(Dimensions.TextProperty); }
			set { this.SetValue(Dimensions.TextProperty, value); }
		}

		

		public bool CanClose {
			get { return (bool)this.GetValue(Dimensions.CanCloseProperty); }
			set { this.SetValue(Dimensions.CanCloseProperty, value); }
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			Button closeButton = (Button)this.GetTemplateChild("CloseButton");
			closeButton.Click += delegate {
				if (this.CloseClicked != null)
					this.CloseClicked(this, EventArgs.Empty);
			};
		}
	}
}

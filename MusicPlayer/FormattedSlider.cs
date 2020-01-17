using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BluePlayer
{
	public class FormattedSlider : Slider
	{
		private ToolTip _autoToolTip;

		public static readonly DependencyProperty ToolTipContentProperty = DependencyProperty.Register("ToolTipContent", typeof(object), typeof(FormattedSlider), new PropertyMetadata(ToolTipContentUpdated));

		public static void ToolTipContentUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FormattedSlider fs = (FormattedSlider)d;
			fs.UpdateToolTip();
		}

		public object ToolTipContent
		{
			get
			{
				return this.GetValue(ToolTipContentProperty);
			}
			set
			{
				this.SetValue(ToolTipContentProperty, value);
			}
		}

		protected override void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
		{
			base.OnThumbDragStarted(e);
			this.UpdateToolTip();
		}

		protected override void OnThumbDragDelta(System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			base.OnThumbDragDelta(e);
			this.UpdateToolTip();
		}

		private void UpdateToolTip()
		{
			if (this.AutoToolTip != null)
			{
				this.AutoToolTip.Content = this.ToolTipContent;
			}
		}

		private ToolTip AutoToolTip
		{
			get
			{
				if (this._autoToolTip == null)
				{
					FieldInfo field = typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
					this._autoToolTip = field.GetValue(this) as ToolTip;
				}
				return this._autoToolTip;
			}
		}
	}
}

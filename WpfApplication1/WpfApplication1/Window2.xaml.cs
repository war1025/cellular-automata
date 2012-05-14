using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1 {

	/**
	 * Clear state dialog
	 **/
	public partial class Window2 : Window {
		public Window2() {
			InitializeComponent();
		}

		/**
		 * Text of the box
		 **/
		public string Text {
			get {
				return textBox1.Text;
			}
		}

		/**
		 * Set dialog result to true so the dialog knows to close
		 **/
		private void ok_Click(object sender, RoutedEventArgs e) {
			if (textBox1.Text != null) {
				DialogResult = true;
			}
		}
	}

}

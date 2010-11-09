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
	/// <summary>
	/// Interaction logic for Window2.xaml
	/// </summary>
	public partial class Window2 : Window {
		public Window2() {
			InitializeComponent();
		}

		public string Text {
			get {
				return textBox1.Text;
			}
		}

		private void ok_Click(object sender, RoutedEventArgs e) {
			if (textBox1.Text != null) {
				DialogResult = true;
			}
		}
	}

	public class StateClearValidationRule : ValidationRule {

		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			uint val;

			if (!uint.TryParse((string)value, out val)) {
				return new ValidationResult(false, "Must be an unsigned integer");
			} else {
				return new ValidationResult(true, null);
			}
		}

	}
}

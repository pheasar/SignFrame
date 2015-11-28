using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SignFrame
{
	/// <summary>
	/// OAuthFrame.xaml の相互作用ロジック
	/// </summary>
	public partial class OAuthFrame : Window
	{
		public string pin { get; set; }

		public OAuthFrame()
		{
			InitializeComponent();
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			pin = PinBox.Text;
			this.Close();
		}

		private void PinBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				pin = PinBox.Text;
				this.Close();
			}
		}
	}
}

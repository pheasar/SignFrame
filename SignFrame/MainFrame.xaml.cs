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
using SharpVectors.Converters;
using CoreTweet;
using System.Windows.Threading;

namespace SignFrame
{
	/// <summary>
	/// Sanct.xaml の相互作用ロジック
	/// </summary>
	public partial class MainFrame : Window
	{
		// 取得ツイート総数
		private int tweetCount;
		// ツイート表示最大数
		private const int tweetMax = 100;

		//public Tokens Program.tokens;
		private User user;
		private TweetItem[] tweetItem;

		public enum Religious
		{
			Asama,
			Sanct,
			Common
		}
		public Religious religious;

		// ツイートのモード
		public enum SendMode
		{
			Tweet,
			Reply,
			TweetWithMedia
		}
		private SendMode sendMode;

		//public List<MentionFrame> mentionFrameList;
		private Image[] pictureBox;
		private List<string> fileLocation;
		private long replyStatusId;
		private long? maxId;

		private Image clickedPictureBox;

		private DispatcherTimer labelTimer;

		private bool isLoading;

		private DateTime now;

		public MainFrame()
		{
			InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TwitterTools.Authentication();

			user = await TwitterTools.UsersShow(Properties.Settings.Default.UserId);
			ProfileImage.Source = new BitmapImage(new Uri(user.ProfileImageUrl.Replace("_normal", "")));
		}
	}
}

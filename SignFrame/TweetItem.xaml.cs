using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Net;

namespace SignFrame
{
	/// <summary>
	/// TweetItem.xaml の相互作用ロジック
	/// </summary>
	public partial class TweetItem : UserControl
	{
		private string innerUrl;
		private MediaEntity[] mediaEntity = new MediaEntity[4];
		private Image[] imageBox = new Image[4];
		private Status status;
		private User user;
		private Tokens tokens;
		private MainFrame mainFrame;
		private Control parentFrame;

		public TweetItem(Status status, Control control)
		{
			InitializeComponent();


			parentFrame = control;

			this.status = status;
			this.user = status.User;
			innerUrl = "";
			imageBox[0] = ImageBox1;
			imageBox[1] = ImageBox2;
			imageBox[2] = ImageBox3;
			imageBox[3] = ImageBox4;

			if (status.Source.IndexOf("<a") >= 0)
			{
				Match m = TwitterTools.viaReg(status.Source);
				status.Source = m.Groups["via"].Value;
			}

			// 画像が含まれていたら表示する
			if (status.Entities.Media != null)
			{
				status.Text = status.Text.Replace(status.Entities.Media[0].Url, "");
				innerUrl = status.Entities.Media[0].ExpandedUrl;

				if (status.ExtendedEntities.Media != null)
				{
					if (status.ExtendedEntities.Media[0].Type == "animated_gif")
					{
						StartButton.BackColor = Color.Transparent;
						StartButton.Parent = ImageBox1;
						StartButton.Visible = true;
					}

					for (int i = 0; i < status.ExtendedEntities.Media.Length; i++)
					{
						mediaEntity[i] = status.ExtendedEntities.Media[i];
						imageBox[i].Source = new BitmapImage(new Uri(status.ExtendedEntities.Media[i].MediaUrl + ":small"));
					}

				}
			}

			// 画像のないPictureBoxを消す
			for (int i = 0; i < imageBox.Length; i++)
			{
				if (imageBox[i].Source == null)
				{
					grid.Children.Remove(imageBox[i]);
				}
			}

			//URLを置換
			foreach (var url in status.Entities.Urls)
			{
				Match m = TwitterTools.urlReg(status.Entities.Urls.ToString());
				status.Text = status.Text.Replace(url.Url, url.DisplayUrl);
				innerUrl = url.ExpandedUrl;
			}

			//リツイート→赤、リプライ→緑、自分宛→青
			if (status.RetweetedStatus != null)
			{
				BackColor = Color.FromArgb(50, 16, 16);

				ProfileImage.ImageLocation = status.RetweetedStatus.User.ProfileImageUrl.Replace("_normal", "");
				NameLabel.Content = status.RetweetedStatus.User.Name + " @" + status.RetweetedStatus.User.ScreenName;
				TextLabel.Content = status.RetweetedStatus.Text;

				RetweetProfileImage.ImageLocation = status.User.ProfileImageUrl.Replace("_normal", "");
				RetweetNameLabel.Content = "Retweeted by " + status.User.Name + " and " + status.RetweetCount.ToString() + " Users";
			}
			else
			{
				ProfileImage.ImageLocation = status.User.ProfileImageUrl.Replace("_normal", "");
				NameLabel.Content = status.User.Name + " @" + status.User.ScreenName;
				TextLabel.Content = WebUtility.HtmlDecode(status.Text);

				grid.Children.Remove(RetweetProfileImage);
				grid.Children.Remove(RetweetNameLabel);
			}

			if (TwitterTools.IsReply(status))
			{
				if (TwitterTools.IsReplyToMe(status))
				{
					BackColor = Color.FromArgb(16, 16, 60);
				}
				else
				{
					BackColor = Color.FromArgb(12, 55, 16);
				}
			}

			switch (parentFrame.Name)
			{
				case "MainFrame":
					mainFrame = (MainFrame)control.TopLevelControl;

					TimeLabel.Content = status.CreatedAt.LocalDateTime + " (via " + status.Source + ")";
					TimeLabel.Location = new Point(59, TextLabel.Location.Y + TextLabel.Size.Height + 3);

					// 画像位置調整
					for (int i = 0; i < ImageBox.Length; i++)
					{
						if (imageBox[i] != null)
						{
							switch (i)
							{
								case 0:
									ImageBox1.Location = new Point(60, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
									break;
								case 1:
									ImageBox2.Location = new Point(216, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
									break;
								case 2:
									ImageBox3.Location = new Point(60, ImageBox1.Location.Y + ImageBox1.Size.Height + 3);
									break;
								case 3:
									ImageBox4.Location = new Point(216, ImageBox2.Location.Y + ImageBox2.Size.Height + 3);
									break;
								default:
									break;
							}
							StartButton.Location = new Point(50, 15);
						}
					}

					if (status.IsFavorited.Value) FavoriteIcon.Image = Properties.Resources.favorite_b;

					break;

				case "ProfileFrame":
				case "MentionFrame":
					MaximumSize = new Size(240, 5000);
					ProfileImage.Size = new Size(30, 30);

					NameLabel.Location = new Point(35, 3);

					TextLabel.Location = new Point(35, 18);
					TextLabel.MinimumSize = new Size(205, 15);
					TextLabel.MaximumSize = new Size(205, 4500);

					TimeLabel.Text = status.CreatedAt.LocalDateTime + "\n(via " + status.Source + ")";
					TimeLabel.Location = new Point(35, TextLabel.Location.Y + TextLabel.Size.Height + 3);

					ReplyIcon.Location = new Point(170, TextLabel.Location.Y + TextLabel.Size.Height + 3);

					RetweetIcon.Location = new Point(193, TextLabel.Location.Y + TextLabel.Size.Height + 3);

					FavoriteIcon.Location = new Point(216, TextLabel.Location.Y + TextLabel.Size.Height + 3);

					// 画像位置調整
					for (int i = 0; i < ImageBox.Length; i++)
					{
						if (imageBox[i] != null)
						{
							imageBox[i].Size = new Size(120, 64);
							switch (i)
							{
								case 0:
									ImageBox1.Location =
										new Point(ProfileImage.Location.X, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
									break;
								case 1:
									ImageBox2.Location =
										new Point(ImageBox1.Location.X + ImageBox1.Size.Width + 4, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
									break;
								case 2:
									ImageBox3.Location =
										new Point(ImageBox1.Location.X, ImageBox1.Location.Y + ImageBox1.Size.Height + 3);
									break;
								case 3:
									ImageBox4.Location =
										new Point(ImageBox3.Location.X + ImageBox3.Size.Width + 4, ImageBox1.Location.Y + ImageBox1.Size.Height + 3);
									break;
								default:
									break;
							}
							StartButton.Location = new Point(35, 7);
						}
					}

					break;

				default:
					break;
			}

			// リツイートユーザー情報の位置調整
			if (status.Entities.Media != null && status.ExtendedEntities.Media != null)
			{
				switch (status.ExtendedEntities.Media.Length)
				{
					case 1:
					case 2:
						RetweetProfileImage.Location = new Point(3, ImageBox1.Location.Y + ImageBox1.Size.Height + 3);
						RetweetNameLabel.Location = new Point(40, ImageBox1.Location.Y + ImageBox1.Size.Height + 3);
						break;
					case 3:
					case 4:
						RetweetProfileImage.Location = new Point(3, ImageBox3.Location.Y + ImageBox3.Size.Height + 3);
						RetweetNameLabel.Location = new Point(40, ImageBox3.Location.Y + ImageBox3.Size.Height + 3);
						break;
				}
			}
			else
			{
				RetweetProfileImage.Location = new Point(3, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
				RetweetNameLabel.Location = new Point(40, TimeLabel.Location.Y + TimeLabel.Size.Height + 3);
			}
		}
	}
}

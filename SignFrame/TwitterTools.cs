using CoreTweet;
using CoreTweet.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SignFrame
{
	static partial class TwitterTools
    {
        private static Properties.Settings settings;
		public static Tokens tokens;
		//public static List<MentionFrame> mentionFrameList;

		static TwitterTools()
        {
			settings = Properties.Settings.Default;
			//mentionFrameList = new List<MentionFrame>();
		}

		//OAuth認証
		public static void Authentication()
		{
			//Properties.Settings.Default.Reset();
			if (!string.IsNullOrEmpty(Properties.Settings.Default.AccessToken)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.AccessTokenSecret))
			{
				tokens = Tokens.Create(
					Properties.Settings.Default.ConsumerKey,
					Properties.Settings.Default.ConsumerSecret,
					Properties.Settings.Default.AccessToken,
					Properties.Settings.Default.AccessTokenSecret);
			}
			else
			{
				var session = OAuth.Authorize(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret);

				Process.Start(session.AuthorizeUri.ToString());

				OAuthFrame oauthFrame = new OAuthFrame();
				oauthFrame.ShowDialog();

				try
				{
					tokens = OAuth.GetTokens(session, oauthFrame.pin);

					Properties.Settings.Default.AccessToken = tokens.AccessToken;
					Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
					Properties.Settings.Default.UserId = tokens.UserId;
					Properties.Settings.Default.ScreenName = tokens.ScreenName;
				}
				catch (FormatException)
				{
					MessageBox.Show("PINコードを正しく入力してください");
					Environment.Exit(0);
				}

				Properties.Settings.Default.Save();
			}

			//Application.Run(new MainFrame());
			//Application.Run(new AsamaFrame());
		}

		// ユーザー情報取得
		public async static Task<User> UsersShow(long? user_id = null, string screen_name = null)
		{
			if (tokens != null)
			{
				User user = null;

				try
				{
					if (user_id != null)
					{
						user = await tokens.Users.ShowAsync(user_id: (long)user_id);
					}
					else if (screen_name != null)
					{
						user = await tokens.Users.ShowAsync(screen_name: screen_name);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}

				if (user != null)
				{
					return user;
				}
				else
				{
					MessageBox.Show("照会失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// ツイートを奏神
		public async static Task<Status> UpdateStatus(string text, long? reply_status_id = null, List<string> media_info = null)
		{
			if (tokens != null)
			{
				Status updatedStatus;

				try
				{
					if (media_info == null)
					{
						updatedStatus = await tokens.Statuses.UpdateAsync(status: text, in_reply_to_status_id: reply_status_id);
					}
					else
					{
						MediaUploadResult[] res = await Task.WhenAll(
							media_info.Where(x => x != "").Select(x => tokens.Media.UploadAsync(new FileInfo(x)))
						);
						updatedStatus = await tokens.Statuses.UpdateAsync(status: text, in_reply_to_status_id: reply_status_id, media_ids: res.Select(x => x.MediaId));
					}

					return updatedStatus;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// リツイートする
		public static async Task<bool> RetweetStatus(Status status)
		{
			if (tokens != null)
			{
				try
				{
					Status updatedStatus = await tokens.Statuses.RetweetAsync(status.Id);

					if (updatedStatus != null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		// DM送る
		public static async void SendDirectMessage(string screenName, string text)
		{
			DirectMessage directMessage = await tokens.DirectMessages.NewAsync(screenName, text);

			//if (res.Result == RequestResult.Success)
			//{

			//}
			//else
			//{
			//	MessageBox.Show("奏神失敗");
			//}
		}

		// ふぁぼる
		public static async Task<bool> CreateFavotire(long statusId)
		{
			if (tokens != null)
			{
				try
				{
					StatusResponse res = await tokens.Favorites.CreateAsync(id => statusId);

					if (res == null)
					{
						MessageBox.Show("御気入登録失敗");
						return false;
					}
					else
					{
						return true;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		// ツイートを消す
		public static async Task<bool> DestroyStatus(Status status)
		{
			if (tokens != null)
			{
				try
				{
					StatusResponse res = await tokens.Statuses.DestroyAsync(status.Id);

					if (res == null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				catch (TwitterException e)
				{
					Console.WriteLine(e.Message);
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		// ホームタイムライン取得
		public static async Task<ListedResponse<Status>> GetHomeTimeline(long? maxId = null)
		{
			if (tokens != null)
			{
				ListedResponse<Status> statuses = null;

				try
				{
					if (maxId == null)
					{
						statuses = await tokens.Statuses.HomeTimelineAsync();
					}
					else
					{
						statuses = await tokens.Statuses.HomeTimelineAsync(max_id: maxId);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}

				if (statuses != null)
				{
					return statuses;
				}
				else
				{
					MessageBox.Show("タイムライン取得失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// ユーザータイムライン取得
		public static async Task<ListedResponse<Status>> GetUserTimeline(long? userId, long? maxId = null)
		{
			if (tokens != null)
			{
				ListedResponse<Status> statuses = null;

				try
				{
					if (userId != null)
					{
						statuses = await tokens.Statuses.UserTimelineAsync(user_id: (long)userId, max_id: maxId);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}

				if (statuses != null)
				{
					return statuses;
				}
				else
				{
					MessageBox.Show("取得失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// ユーザーのお気に入りを取得
		public static async Task<ListedResponse<Status>> FavoritesList(long? userId, long? maxId = null)
		{
			if (tokens != null)
			{
				ListedResponse<Status> favorites = null;

				try
				{
					if (userId != null)
					{
						favorites = await tokens.Favorites.ListAsync(id: (long)userId, max_id: maxId);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}

				if (favorites != null)
				{
					return favorites;
				}
				else
				{
					MessageBox.Show("取得失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// ユーザーのフォロワーを取得
		public static async Task<Cursored<User>> FollowersList(long? userId, long? cursor = null)
		{
			if (tokens != null)
			{
				Cursored<User> users = null;

				try
				{
					if (userId != null)
					{
						users = await tokens.Followers.ListAsync(user_id: (long)userId, cursor: cursor, count: 100);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}

				if (users != null)
				{
					return users;
				}
				else
				{
					MessageBox.Show("取得失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// ユーザーのフォロワーを取得
		public static async Task<Cursored<User>> FriendList(long? userId, long? cursor = null)
		{
			if (tokens != null)
			{
				Cursored<User> users = null;

				try
				{
					if (userId != null)
					{
						users = await tokens.Friends.ListAsync(user_id: (long)userId, cursor: cursor, count: 100);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return null;
				}

				if (users != null)
				{
					return users;
				}
				else
				{
					MessageBox.Show("取得失敗");
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		// リプライかどうか
		public static bool IsReply(Status status)
        {
            if (status.InReplyToUserId != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		// 自分宛てのリプライかどうか
        public static bool IsReplyToMe(Status status)
        {
            if (status.InReplyToUserId == Properties.Settings.Default.UserId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //via取り出し用正規表現
		public static Match viaReg(string via)
		{
			string pattern = @"<a.*?>(?<via>.*?)</a>";
			return Regex.Match(via, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
		//URL取り出し用正規表現
		public static Match urlReg(string via)
		{
			string pattern = @"http(s)?://([\da-zA-Z]+\.)+[\da-zA-Z]+(/[\da-zA-Z./?%&=_]*)?";
			return Regex.Match(via, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		public static Match tcoReg(string via)
		{
			string pattern = @"http(s)?://t\.co/[\da-zA-Z]+";
			return Regex.Match(via, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
		//ScreenName取り出し用正規表現
		public static Match screenNameReg(string via)
		{
			string pattern = @"@(?<srcName>[a-zA-Z0-9]*)";
			return Regex.Match(via, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}
    }

}


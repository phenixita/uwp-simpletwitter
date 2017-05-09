using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Buongiorno
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BtnGetTimeline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UniversalAuthorizer auth = await Authenticate();

                using (var twitterCtx = new TwitterContext(auth))
                {
                    var srch = await
                                   (from tweet in twitterCtx.Status
                                    where tweet.Type == StatusType.Home
                                    select tweet).ToListAsync();

                    var observableTweets = new ObservableCollection<Status>(srch);
                    
                    TweetList.DataContext = observableTweets;
                }
            }
            catch (Exception ex)
            {
                var msg = new MessageDialog(ex.Message, "Ops!");
                await msg.ShowAsync();
            }
        }

        private static async Task<UniversalAuthorizer> Authenticate()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var auth = new UniversalAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = "---",
                    ConsumerSecret = "---",

                    OAuthToken = localSettings.Values["OAuthToken"]?.ToString(),
                    OAuthTokenSecret = localSettings.Values["OAuthTokenSecret"]?.ToString(),
                    ScreenName = localSettings.Values["ScreenName"]?.ToString(),
                    UserID = Convert.ToUInt64(localSettings.Values["UserId"] ?? 0)
                },
                Callback = "http://127.0.0.1"
            };

            await auth.AuthorizeAsync();

            //Save credentials.
            localSettings.Values["OAuthToken"] = auth.CredentialStore.OAuthToken;
            localSettings.Values["OAuthTokenSecret"] = auth.CredentialStore.OAuthTokenSecret;
            localSettings.Values["ScreenName"] = auth.CredentialStore.ScreenName;
            localSettings.Values["UserId"] = auth.CredentialStore.UserID;
            return auth;
        }

        private async void btnSendTweet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserTweet.Text)) return;

            try
            {
                var tweetText = txtUserTweet.Text;

                UniversalAuthorizer auth = await Authenticate();

                using (var twitterCtx = new TwitterContext(auth))
                {
                    await twitterCtx.TweetAsync(tweetText);

                    await new MessageDialog("You Tweeted: " + tweetText, "Success!").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "Ops!").ShowAsync();
            }
        }
    }
}

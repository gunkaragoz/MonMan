using Microsoft.WindowsAzure.MobileServices;
using MonMan;
using MonMan.Common;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MonMan
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    /// 

    public sealed partial class LoginPage : Page
    {

        private MobileServiceCollection<userItem, userItem> userItems;
        private IMobileServiceTable<userItem> usersTable = App.MobileService.GetTable<userItem>();

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public class userItem
        {
            public string Id { get; set; }

            [JsonProperty(PropertyName = "username")]

            public string Username { get; set; }

        }


        public LoginPage()
        {
            //this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private userItem appUser = new userItem();

        public string appUserName;

        private async void fbLoginTapped(object sender, TappedRoutedEventArgs e)
        {
            //await AuthenticateFacebook();            

            await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
            var userId = App.MobileService.CurrentUser.UserId;
            var facebookId = userId.Substring(userId.IndexOf(':') + 1);
            var client = new HttpClient();
            var fbUser = await client.GetAsync("https://graph.facebook.com/" + facebookId);
            var response = await fbUser.Content.ReadAsStringAsync();
            var jo = JsonObject.Parse(response);
            var userName = jo["name"].GetString();
            //this.pageTitle.Text = "Welcome " + userName;

            appUserName = userName;

            //var newUser = new userItem { Id = userId, Username = userName };
            appUser.Id = userId;
            appUser.Username = userName;


            if (userName != null)
            {
                try
                {
                    var results = await usersTable
                                    .Where(appuser => appuser.Username == appUser.Username)
                                    .ToListAsync();
                    if (results.Count == 0)
                    {
                        await usersTable.InsertAsync(appUser);
                    }
                    else
                    {
                        var dialog = new MessageDialog("User already registered!");
                        dialog.Commands.Add(new UICommand("OK"));
                        await dialog.ShowAsync();
                    }
                    Frame.Navigate(typeof(MainPage));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            
            //await usersTable.InsertAsync(newUser);
            //userItems.Add(newUser);

            /*var table = App.MobileService.GetTable("userItems");
            var response1 = await table.ReadAsync("");
            var identities = response1.GetArray()[0].GetObject(); */

        }

        private async void twLogin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await AuthenticateTwitter();
        }

        private async void InsertUserItem(userItem userItem)
        {

            await usersTable.InsertAsync(userItem);
            userItems.Add(userItem);
            
        }

        private MobileServiceUser user;
        private async System.Threading.Tasks.Task AuthenticateFacebook(MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.Facebook)
        {

            userItems = await usersTable
              .ToCollectionAsync();

            if (user == null)
            {

                try
                {

                    await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                    var userId = App.MobileService.CurrentUser.UserId;
                    var facebookId = userId.Substring(userId.IndexOf(':') + 1);
                    var client = new HttpClient();
                    var fbUser = await client.GetAsync("https://graph.facebook.com/" + facebookId);
                    var response = await fbUser.Content.ReadAsStringAsync();
                    var jo = JsonObject.Parse(response);
                    var userName = jo["name"].GetString();
                    //this.lblTitle.Text = "Multi-auth Blog: " + userName;

                    //InsertUserItem(appUser);
                }
                catch (InvalidOperationException)
                {

                    
                }
                
            }

        }


        private async System.Threading.Tasks.Task AuthenticateTwitter(MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.Twitter)
        {

            userItems = await usersTable.ToCollectionAsync();

            if (user == null)
            {
                string message;
                try
                {
                    user = await App.MobileService.LoginAsync(provider);

                    message =
                        string.Format("You are now logged in - {0}", user.UserId);
                    var userItem = new userItem { Id = user.UserId, Username = "" };
                    InsertUserItem(userItem);
                    //dialog = new MessageDialog(message);
                    //dialog.Commands.Add(new UICommand("OK"));
                    //await dialog.ShowAsync();
                    Frame.Navigate(typeof(MainPage));
                }
                catch (InvalidOperationException)
                {
                    message = "You must log in. Login Required";
                }

                //extend

                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();
            }
        }

            }
        }

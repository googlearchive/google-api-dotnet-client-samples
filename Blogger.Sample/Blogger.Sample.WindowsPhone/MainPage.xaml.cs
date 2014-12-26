/*
Copyright 2014 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Blogger.Common;
using Blogger.Sample.Repository;
using Blogger.Sample.ViewModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Blogger.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IWebAuthenticationContinuable
    {
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            // In a real application consider injecting your VM and repository.
            this.DataContext = new BlogsViewModel(new BloggerRepository());

            Current = this;
        }

        /// <summary>
        /// Continues the app after retrieving the authorization code.
        /// 
        /// First it stores the authorization code result in the data store.
        /// Then it calls the view model get blogs method, in order to retrieve all blogs.
        /// The view model is responsible to check for if the authorization code exists in the data store, and then,
        /// continue with the regular flow.
        /// After retrieving all blogs, this method deletes the authorization code so it won't be used in a next run.
        /// </summary>
        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            await PasswordVaultDataStore.Default.StoreAsync<SerializableWebAuthResult>(
                SerializableWebAuthResult.Name, new SerializableWebAuthResult(args.WebAuthenticationResult));
            await ((BlogsViewModel)this.DataContext).GetBlogsAsync();
            await PasswordVaultDataStore.Default.DeleteAsync<SerializableWebAuthResult>(
                SerializableWebAuthResult.Name);
        }
    }
}
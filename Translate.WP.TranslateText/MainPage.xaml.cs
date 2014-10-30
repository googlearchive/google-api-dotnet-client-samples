/*
Copyright 2014 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Google.Apis.Translate.v2;
using Google.Apis.Services;

using Translate.WP.TranslateText.Common;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Translate.WP.TranslateText
{
    /// <summary>
    /// The main page of the sample app, which can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        // To run this sample, put your own API key here.
        private const string API_KEY = "YOUR API KEY GOES HERE";

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.DefaultViewModel["OriginalText"] = "Octopus pants have eight legs.";
            this.DefaultViewModel["TargetLanguage"] = "de";

            if (e.PageState != null)
            {
                // Restore the previously saved state.
                foreach (var pair in e.PageState)
                {
                    this.DefaultViewModel[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // Save all the state from our view model.
            foreach (var pair in this.DefaultViewModel)
            {
                e.PageState[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Asynchronously calls the Translate API to translate a string from one language to
        /// another.
        /// </summary>
        /// <param name="service">An instance of the translate service.</param>
        /// <param name="text">The string to translate.</param>
        /// <param name="sourceLanguage">The code for the source language.</param>
        /// <param name="targetLanguage">The code for the target language.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// For a list of supported language codes, see
        /// https://cloud.google.com/translate/v2/using_rest#language-params.
        /// </remarks>
        private static async Task<string> TranslateTextAsync(TranslateService service, string text,
            string sourceLanguage, string targetLanguage)
        {
            var request = service.Translations.List(new[] { text }, targetLanguage);
            request.Source = sourceLanguage;
            request.Format = TranslationsResource.ListRequest.FormatEnum.Text;

            var response = await request.ExecuteAsync();
            return response.Translations[0].TranslatedText;
        }

        private async void TranslateBtn_Click(object sender, RoutedEventArgs e)
        {
            // In this sample, we create the service each time the user clicks. In a real application, consider
            // creating the service only once and then reusing the same instance for every request.
            var service = new TranslateService(new BaseClientService.Initializer()
            {
                ApiKey = API_KEY,
                ApplicationName = "Translate WP API Sample",
            });

            // Execute the first translation request.
            var srcText = (string)this.DefaultViewModel["OriginalText"];
            var targetLanguage = (string)this.DefaultViewModel["TargetLanguage"];
            var translatedText = await TranslateTextAsync(service, srcText, "en", targetLanguage);

            this.DefaultViewModel["TranslatedText"] = translatedText;

            // Translate the text back to English to verify that the translation is right.
            var reTranslatedText = await TranslateTextAsync(service, translatedText, targetLanguage, "en");
            this.DefaultViewModel["ReTranslatedText"] = reTranslatedText;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}

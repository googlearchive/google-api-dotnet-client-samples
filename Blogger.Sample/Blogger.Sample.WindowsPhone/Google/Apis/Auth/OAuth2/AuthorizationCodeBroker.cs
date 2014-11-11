// This file will be added to a Google.Apis.Auth (WP8.1) core project starting from release 1.9.1.

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

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;

namespace Google.Apis.Auth.OAuth2
{
    /// <summary>
    /// OAuth 2.0 verification code receiver for Windows Phone 8.1 application that opens an embedded Google account
    /// form to enter the user's credentials and accepts the application access to its token.
    /// </summary>
    public class AuthorizationCodeBroker : ICodeReceiver
    {
        #region ICodeReceiver Members

        public string RedirectUri
        {
            get { return GoogleAuthConsts.LocalhostRedirectUri; }
        }

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            TaskCompletionSource<AuthorizationCodeResponseUrl> tcs =
                new TaskCompletionSource<AuthorizationCodeResponseUrl>();
            await ReceiveCodeAsync(url, tcs);
            return tcs.Task.Result;
        }

        private async Task InvokeFromUIThread(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => action());
        }

        /// <summary>Asynchronously receives the authorization code.</summary>
        /// <param name="url">The authorization code request URL.</param>
        /// <param name="tcs">Task completion source whose result will be set to the authorization code.</param>
        private async Task ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            TaskCompletionSource<AuthorizationCodeResponseUrl> tcs)
        {
            var result = await PasswordVaultDataStore.Default.GetAsync<WebAuthResult>(WebAuthResult.Name);
            if (result == null)
            {
                // We should run WebAuthenticationBroker.AuthenticateAndContinue from the UI thread ONLY.
                await InvokeFromUIThread(() => WebAuthenticationBroker.AuthenticateAndContinue(url.Build(),
                    new Uri(GoogleAuthConsts.LocalhostRedirectUri), null, WebAuthenticationOptions.None));
                
                // No need to return anything, cause the application is going to be suspended now.
                return;
            }

            const string Code = "code=";
            const string Error = "error=";
            // Get the index of the error or the code.
            var index = result.ResponseData.IndexOf(Code);
            index = index != -1 ? index : result.ResponseData.IndexOf(Error);

            if (index != -1)
            {
                tcs.SetResult(new AuthorizationCodeResponseUrl(result.ResponseData.Substring(index)));
                return;
            }

            tcs.SetException(new TokenResponseException(
                new TokenErrorResponse
                {
                    Error = result.ResponseStatus.ToString(),
                    ErrorDescription = "The WebAuthenticationBroker didn't return a code or an error. Details: " +
                        result.ResponseErrorDetail,
                }));
        }

        #endregion
    }
}
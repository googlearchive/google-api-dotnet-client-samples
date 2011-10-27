/*
Copyright 2011 Google Inc

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

using Google.Apis.Samples.Helper;

namespace TasksExample.WinForms.NoteMgr
{
    /// <summary>
    /// This class provides the client credentials for all the samples in this solution.
    /// In order to run all of the samples, you have to enable API access for every API 
    /// you want to use, enter your credentials here.
    /// 
    /// You can find your credentials here:
    ///  https://code.google.com/apis/console/#:access
    /// 
    /// For your own application you should find a more secure way than just storing your client secret inside a string,
    /// as it can be lookup up easily using a reflection tool.
    /// </summary>
    internal static class ClientCredentials
    {
        /// <summary>
        /// The OAuth2.0 Client ID of your project.
        /// </summary>
        public static readonly string ClientID = "<Enter your ClientID here>";

        /// <summary>
        /// The OAuth2.0 Client secret of your project.
        /// </summary>
        public static readonly string ClientSecret = "<Enter your ClientSecret here>";

        /// <summary>
        /// Your Api/Developer key.
        /// </summary>
        public static readonly string ApiKey = "<Enter your ApiKey here>";

        #region Verify Credentials
        static ClientCredentials()
        {
            ReflectionUtils.VerifyCredentials(typeof(ClientCredentials));
        }
        #endregion
    }
}

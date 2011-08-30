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

using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Samples.Helper.NativeAuthorizationFlows;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Authorization helper for Native Applications.
    /// </summary>
    public static class AuthorizationMgr
    {
        private static readonly INativeAuthorizationFlow[] NativeFlows = new INativeAuthorizationFlow[]
                                                                        {
                                                                            new LoopbackServerAuthorizationFlow(), 
                                                                            new WindowTitleNativeAuthorizationFlow() 
                                                                        };

        /// <summary>
        /// Requests authorization on a native client by using a predefined set of authorization flows.
        /// </summary>
        /// <param name="client">The client used for authentication.</param>
        /// <param name="authState">The requested authorization state.</param>
        /// <returns>The authorization code, or null if cancelled by the user.</returns>
        /// <exception cref="NotSupportedException">Thrown if no supported flow was found.</exception>
        public static string RequestNativeAuthorization(NativeApplicationClient client, IAuthorizationState authState)
        {
            // Try each available flow until we get an authorization / error.
            foreach (INativeAuthorizationFlow flow in NativeFlows)
            {
                try
                {
                    return flow.RetrieveAuthorization(client, authState);
                } 
                catch (NotSupportedException) { /* Flow unsupported on this environment */ }
            }

            throw new NotSupportedException("Found no supported native authorization flow.");
        }

        /// <summary>
        /// Requests authorization on a native client by using a predefined set of authorization flows.
        /// </summary>
        /// <param name="client">The client used for authorization.</param>
        /// <param name="scopes">The requested set of scopes.</param>
        /// <returns>The authorized state.</returns>
        /// <exception cref="AuthenticationException">Thrown if the request was cancelled by the user.</exception>
        public static IAuthorizationState RequestNativeAuthorization(NativeApplicationClient client,
                                                                     params string[] scopes)
        {
            IAuthorizationState state = new AuthorizationState(scopes);
            string authCode = RequestNativeAuthorization(client, state);

            if (string.IsNullOrEmpty(authCode))
            {
                throw new AuthenticationException("The authentication request was cancelled by the user.");
            }

            return client.ProcessUserAuthorization(authCode, state);
        }

        /// <summary>
        /// Returns a cached refresh token for this application, or null if unavailable.
        /// </summary>
        /// <param name="storageName">The file name (without extension) used for storage.</param>
        /// <param name="key">The key to decrypt the data with.</param>
        /// <returns>The authorization state containing a Refresh Token, or null if unavailable</returns>
        public static AuthorizationState GetCachedRefreshToken(string storageName,
                                                               string key)
        {
            string file = storageName + ".auth";
            byte[] contents = AppData.ReadFile(file);

            if (contents == null)
            {
                return null; // No cached token available.
            }

            byte[] salt = Encoding.Unicode.GetBytes(Assembly.GetEntryAssembly().FullName + key);
            byte[] decrypted = ProtectedData.Unprotect(contents, salt, DataProtectionScope.CurrentUser);
            string[] content = Encoding.Unicode.GetString(decrypted).Split(new[] { "\r\n" }, StringSplitOptions.None);
         
            // Create the authorization state.
            string[] scopes = content[0].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            string refreshToken = content[1];
            return new AuthorizationState(scopes) { RefreshToken = refreshToken };
        }
        
        /// <summary>
        /// Saves a refresh token to the specified storage name, and encrypts it using the specified key.
        /// </summary>
        public static void SetCachedRefreshToken(string storageName,
                                                 string key,
                                                 IAuthorizationState state)
        {
            // Create the file content.
            string scopes = state.Scope.Aggregate("", (left, append) => left + " " + append);
            string content = scopes + "\r\n" + state.RefreshToken;

            // Encrypt it.
            byte[] salt = Encoding.Unicode.GetBytes(Assembly.GetEntryAssembly().FullName + key);
            byte[] encrypted = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(content), salt, DataProtectionScope.CurrentUser);

            // Save the data to the auth file.
            string file = storageName + ".auth";
            AppData.WriteFile(file, encrypted);
        }
    }
}
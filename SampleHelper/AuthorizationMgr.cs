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
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Samples.Helper.Forms;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Authorization helper class.
    /// </summary>
    public static class AuthorizationMgr
    {
        /// <summary>
        /// Requests an authorization code by using the specified authorization URL.
        /// Implements the Native-Application-Flow by opening a Request-Form and the browser.
        /// </summary>
        /// <param name="authUri">The URL where the authorization code can be retrieved.</param>
        /// <returns>The authorization code, or throws an AuthenticationException if the request failed.</returns>
        public static string RequestAuthorization(Uri authUri)
        {
            if (!Application.RenderWithVisualStyles)
            {
                Application.EnableVisualStyles();
            }

            Application.DoEvents();
            string authCode = OAuth2AuthorizationDialog.ShowDialog(authUri);
            Application.DoEvents();

            if (string.IsNullOrEmpty(authCode))
            {
                throw new AuthenticationException("Authentication request cancelled by user.");
            }

            return authCode;
        }

        /// <summary>
        /// Returns a cached refresh token for this application, or null if unavailable.
        /// </summary>
        public static AuthorizationState GetCachedRefreshToken(string storageName, string key, params string[] requiredScopes)
        {
            string file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), storageName + ".auth");

            if (!File.Exists(file))
            {
                return null;
            }

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = GetCompatibleKey(key, des);
            des.IV = des.Key;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (CryptoStream cryptoStream = new CryptoStream(fs, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    var reader = new StreamReader(cryptoStream);
                    string[] scopes = reader.ReadLine().Split(' ');
                    string refreshToken = reader.ReadLine();

                    if (scopes.Intersect(requiredScopes).Count() != requiredScopes.Length)
                    {
                        return null; // Not every scope is covered.
                    }

                    return new AuthorizationState(scopes) { RefreshToken = refreshToken };
                }
            }
        }

        /// <summary>
        /// Saves a refresh token to the specified storage name, and encrypts it using the specified key.
        /// </summary>
        public static void SetCachedRefreshToken(string storageName,
                                                 string key,
                                                 IAuthorizationState state,
                                                 params string[] scopesToAdd)
        {
            // Add granted scopes to the authorization state if missing.
            foreach (string scope in scopesToAdd)
            {
                if (!state.Scope.Contains(scope))
                {
                    state.Scope.Add(scope);
                }
            }

            // Get the auth file name.
            string file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), storageName + ".auth");

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = GetCompatibleKey(key, des);
            des.IV = des.Key;

            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (CryptoStream cryptoStream = new CryptoStream(fs, des.CreateEncryptor(), CryptoStreamMode.Write)
                    )
                {
                    StreamWriter writer = new StreamWriter(cryptoStream);

                    // Save the set of scopes.
                    string scopes = state.Scope.Aggregate("", (left, append) => left + " " + append);
                    writer.WriteLine(scopes);

                    // Save the refresh token.
                    writer.WriteLine(state.RefreshToken);

                    writer.Flush();
                }
            }
        }

        private static byte[] GetCompatibleKey(string inputKey, DESCryptoServiceProvider cryptoService)
        {
            byte[] byteKey = Encoding.ASCII.GetBytes(inputKey);
            byte[] cryptoKey = new byte[cryptoService.BlockSize / 8];
            Array.Copy(byteKey, cryptoKey, Math.Min(byteKey.Length, cryptoKey.Length));
            return cryptoKey;
        }
    }
}
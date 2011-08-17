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
using DotNetOpenAuth.OAuth2;

namespace Google.Apis.Samples.Helper.NativeAuthorizationFlows
{
    /// <summary>
    /// An authorization flow is the process of obtaining an AuthorizationCode 
    /// when provided with an IAuthorizationState.
    /// </summary>
    internal interface INativeAuthorizationFlow
    {
        /// <summary>
        /// Retrieves the authorization of the user for the given AuthorizationState.
        /// </summary>
        /// <param name="client">The client used for authentication.</param>
        /// <param name="authorizationState">The state requested.</param>
        /// <returns>The authorization code, or null if the user cancelled the request.</returns>
        /// <exception cref="NotSupportedException">Thrown if this flow is not supported.</exception>
        string RetrieveAuthorization(UserAgentClient client, IAuthorizationState authorizationState);
    }
}

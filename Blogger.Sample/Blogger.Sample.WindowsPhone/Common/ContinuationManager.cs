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

using Blogger.Sample;

namespace Blogger.Common
{
    /// <summary>
    /// ContinuationManager is used to detect if the most recent activation was due to a authentication continuation.
    /// 
    /// Note: To keep this sample as simple as possible, the content of the file was changed to support
    /// WebAuthenticationBrokerContinuation ONLY. Take a look in http://msdn.microsoft.com/en-us/library/dn631755.aspx
    /// for a full documentation on how to support continuation in other cases.
    /// </summary>
    public class ContinuationManager
    {
        /// <summary>
        /// Sets the ContinuationArgs for this instance.
        /// Should be called by the main activation handling code in App.xaml.cs.
        /// </summary>
        /// <param name="args">The activation args.</param>
        internal void Continue(IContinuationActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.WebAuthenticationBrokerContinuation:
                    var page = MainPage.Current as IWebAuthenticationContinuable;
                    if (page != null)
                    {
                        page.ContinueWebAuthentication(args as WebAuthenticationBrokerContinuationEventArgs);
                    }
                    break;
            }
        }
    }

    /// <summary>Implement this interface if your page invokes the web authentication broker.</summary>
    interface IWebAuthenticationContinuable
    {
        /// <summary>
        /// This method is invoked when the web authentication broker returns with the authentication result.
        /// </summary>
        /// <param name="args">Activated event args object that contains returned authentication token.</param>
        void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args);
    }
}
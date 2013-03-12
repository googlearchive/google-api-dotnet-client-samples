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
using System.Text.RegularExpressions;

using Google;
using Google.Apis.Samples.Helper;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;

namespace UrlShortener.ASP.NET
{
    /// <summary>
    /// ASP.NET UrlShortener Sample
    /// 
    /// This sample makes use of ASP.NET and the UrlShortener API to demonstrate how to do make unauthenticated
    /// requests to an api.
    /// </summary>
    public partial class _Default : System.Web.UI.Page
    {
        private static UrlshortenerService _service;

        protected void Page_Load(object sender, EventArgs e)
        {
            // If we did not construct the service so far, do it now.
            if (_service == null)
            {
                BaseClientService.Initializer initializer = new BaseClientService.Initializer();
                // You can enter your developer key for services requiring a developer key.
                /* initializer.ApiKey = "<Insert Developer Key here>"; */
                _service = new UrlshortenerService(initializer);

            }
        }

        protected void input_TextChanged(object sender, EventArgs e)
        {
            // Change the text of the button according to the content.
            action.Text = IsShortUrl(input.Text) ? "Expand" : "Shorten";
        }

        protected void action_Click(object sender, EventArgs e)
        {
            string url = input.Text;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            // Execute methods on the UrlShortener service based upon the type of the URL provided.
            try
            {
                string resultURL;
                if (IsShortUrl(url))
                {
                    // Expand the URL by using a Url.Get(..) request.
                    Url result = _service.Url.Get(url).Fetch();
                    resultURL = result.LongUrl;
                }
                else
                {
                    // Shorten the URL by inserting a new Url.
                    Url toInsert = new Url { LongUrl = url };
                    toInsert = _service.Url.Insert(toInsert).Fetch();
                    resultURL = toInsert.Id;
                }
                output.Text = string.Format("<a href=\"{0}\">{0}</a>", resultURL);
            }
            catch (GoogleApiException ex)
            {
                output.Text = ex.ToHtmlString();
            }
        }

        private static readonly Regex ShortUrlRegex =
                    new Regex("^http[s]?://goo.gl/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static bool IsShortUrl(string url)
        {
            return ShortUrlRegex.IsMatch(url);
        }
    }
}
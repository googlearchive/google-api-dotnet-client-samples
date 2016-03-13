/*
Copyright 2015 Google Inc

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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Calendar.ASP.NET.MVC5
{
    /// <summary>
    /// Holds the Google API client secrets. Replace the values below with credentials from your developer console
    /// (https://console.developers.google.com).
    /// </summary>
    internal static class MyClientSecrets
    {
        public const string ClientId = "YOUR CLIENT ID HERE";
        public const string ClientSecret = "YOUR CLIENT SECRET HERE";
    }
}

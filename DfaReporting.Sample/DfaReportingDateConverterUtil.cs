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

namespace DfaReporting.Sample
{
    /// <summary>
    /// This simple utility converts <c>DateTime</c> objects to the <c>string</c> format that the DFA Reporting API 
    /// always expects dates to be in.
    /// </summary>
    internal static class DfaReportingDateConverterUtil
    {
        private const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Takes a <c>DateTime</c> object and converts it to the proper <c>string</c> format for the 
        /// Dfa Reporting API.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <returns>The given date in the proper format.</returns>
        public static string convert(DateTime date)
        {
            return date.ToString(DateFormat);
        }
    }
}

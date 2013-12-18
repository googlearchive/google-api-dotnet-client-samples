/*
Copyright 2012 Google Inc

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

using Google.Apis.Dfareporting.v1_2;
using Google.Apis.Dfareporting.v1_2.Data;

namespace DfaReporting.Sample
{
    /// <summary>
    /// This example gets the first page of a particular type of dimension available for reporting in
    /// the given date range. You can use a similar workflow to retrieve the values for any dimension.
    /// 
    /// The available dimension value names can be found here:
    /// https://developers.google.com/doubleclick-advertisers/reporting/v1.1/dimensions
    /// </summary>
    internal class GetDimensionValuesHelper
    {
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for retrieving dimension values.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GetDimensionValuesHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Lists the first page of results for a dimension value.
        /// </summary>
        /// <param name="dimensionName">The name of the dimension to retrieve values for.</param>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="startDate">Values which existed after this start date will be returned.</param>
        /// <param name="endDate">Values which existed before this end date will be returned.</param>
        /// <param name="maxPageSize">The maximum page size to retrieve.</param>
        /// <returns>The first page of dimension values received.</returns>
        public DimensionValueList Query(string dimensionName, long userProfileId, DateTime startDate,
            DateTime endDate, int maxPageSize)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing available {0} values", dimensionName);
            Console.WriteLine("=================================================================");

            // Create a dimension value query which selects available dimension values.
            var request = new DimensionValueRequest();
            request.DimensionName = dimensionName;
            request.StartDate = DfaReportingDateConverterUtil.convert(startDate);
            request.EndDate = DfaReportingDateConverterUtil.convert(endDate);
            var dimensionQuery = service.DimensionValues.Query(request, userProfileId);
            dimensionQuery.MaxResults = maxPageSize;

            // Retrieve values and display them.
            var values = dimensionQuery.Execute();

            if (values.Items.Count > 0)
            {
                foreach (var dimensionValue in values.Items)
                {
                    Console.WriteLine("{0} with value \"{1}\" was found.", dimensionName, dimensionValue.Value);
                }
            }
            else
            {
                Console.WriteLine("No values found.");
            }
            Console.WriteLine();
            return values;
        }
    }
}

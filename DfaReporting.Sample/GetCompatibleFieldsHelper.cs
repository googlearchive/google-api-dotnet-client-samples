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

using Google.Apis.Dfareporting.v1_3;
using Google.Apis.Dfareporting.v1_3.Data;

namespace DfaReporting.Sample
{

    /// <summary>
    /// Lists the fields that are compatible with a given standard report.
    /// </summary>
    internal class GetCompatibleFieldsHelper
    {

        private readonly DfareportingService service;

        /// <summary>Instantiate a helper for getting the compatible fields for a standard report.</summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public GetCompatibleFieldsHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>Requests the compatible fields for a specified report.</summary>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="report">The report to request compatible fields for.</param>
        public void Run(long userProfileId, Report report) 
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Getting compatible fields for standard report with ID {0}", report.Id);
            Console.WriteLine("=================================================================");

            var compatibleFields = service.Reports.CompatibleFields.Query(report, userProfileId).Execute();
      
            // Since this is a standard report, we check the "ReportCompatibleFields" propery.
            // For other reports, we would check that report type's specified property.
            var standardReportCompatibleFields = compatibleFields.ReportCompatibleFields;

            foreach (var compatibleDimension in standardReportCompatibleFields.Dimensions)
            {
                Console.WriteLine("Dimension \"{0}\" is compatible.", compatibleDimension.Name);
            }

            foreach (var compatibleMetric in standardReportCompatibleFields.Metrics)
            {
                Console.WriteLine("Metric \"{0}\" is compatible.", compatibleMetric.Name);
            }

            foreach (var compatibleDimensionFilter in standardReportCompatibleFields.DimensionFilters)
            {
                Console.WriteLine("Dimension Filter \"{0}\" is compatible.", compatibleDimensionFilter.Name);
            }

            foreach (var compatibleActivityMetric in standardReportCompatibleFields.PivotedActivityMetrics)
            {
                Console.WriteLine("Pivoted Activity Metric \"{0}\" is compatible.", compatibleActivityMetric.Name);
            }

            Console.WriteLine();
        }

    }
}

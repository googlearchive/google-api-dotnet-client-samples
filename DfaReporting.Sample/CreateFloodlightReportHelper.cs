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
using System.Collections.Generic;

using Google.Apis.Dfareporting.v1_3;
using Google.Apis.Dfareporting.v1_3.Data;

namespace DfaReporting.Sample
{
    /// <summary>
    /// This example creates a simple Floodlight report for the given Floodlight Configuration ID.
    /// </summary>
    internal class CreateFloodlightReportHelper
    {
        private readonly DfareportingService service;

        /// <summary>
        /// Instantiate a helper for creating floodlight reports.
        /// </summary>
        /// <param name="service">DfaReporting service object used to run the requests.</param>
        public CreateFloodlightReportHelper(DfareportingService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Inserts (creates) a simple Floodlight report for a given Floodlight Configuration ID.
        /// </summary>
        /// <param name="userProfileId">The ID number of the DFA user profile to run this request as.</param>
        /// <param name="floodlightConfigId">The Floodlight configuration ID the report is about.</param>
        /// <param name="startDate">The starting date of the report.</param>
        /// <param name="endDate">The ending date of the report.</param>
        /// <returns>The newly created report</returns>
        public Report Insert(long userProfileId, DimensionValue floodlightConfigId, DateTime startDate,
            DateTime endDate)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Creating a new floodlight report for Floodlight config ID {0}",
                floodlightConfigId.Value);
            Console.WriteLine("=================================================================");

            // Create a report.
            Report report = new Report();
            report.Name = string.Format("API Floodlight Report: Floodlight ID {0}", floodlightConfigId.Value);
            report.FileName = "api_floodlight_report_files";
            // Set the type of report you want to create. Available report types can be found in the description of 
            // the type property: https://developers.google.com/doubleclick-advertisers/reporting/v1.1/reports
            report.Type = "FLOODLIGHT";

            // Create criteria.
            var criteria = new Report.FloodlightCriteriaData();
            criteria.DateRange = new DateRange
            {
                StartDate = DfaReportingDateConverterUtil.convert(startDate),
                EndDate = DfaReportingDateConverterUtil.convert(endDate)
            };
            // Set the dimensions, metrics, and filters you want in the report. The available values can be found 
            // here: https://developers.google.com/doubleclick-advertisers/reporting/v1.1/dimensions
            criteria.Dimensions = new List<SortedDimension> {
                new SortedDimension { Name = "dfa:floodlightConfigId" },
                new SortedDimension { Name = "dfa:activity" },
                new SortedDimension { Name = "dfa:advertiser" } };
            criteria.MetricNames = new List<string> {
                "dfa:activityClickThroughConversions",
                "dfa:activityClickThroughRevenue",
                "dfa:activityViewThroughConversions",
                "dfa:activityViewThroughRevenue" };
            criteria.DimensionFilters = new List<DimensionValue> { floodlightConfigId };

            report.FloodlightCriteria = criteria;
            Report result = service.Reports.Insert(report, userProfileId).Execute();
            Console.WriteLine("Created report with ID \"{0}\" and display name \"{1}\"", result.Id, result.Name);
            Console.WriteLine();
            return result;
        }
    }
}

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Reflection Helper
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Tries to return a descriptive name for the specified member info. 
        /// Uses the DescriptionAttribute if available.
        /// </summary>
        /// <returns>Description from DescriptionAttriute or name of the MemberInfo</returns>
        public static string GetDescriptiveName(MemberInfo info)
        {
            // If available: Return the description set in the DescriptionAttribute
            foreach (DescriptionAttribute attribute in info.GetCustomAttributes(typeof(DescriptionAttribute), true))
            {
                return attribute.Description;
            }

            // Otherwise: Return the name of the member
            return info.Name;
        }

        /// <summary>
        /// Selects all type members from the collection which have the specified argument.
        /// </summary>
        /// <typeparam name="TMemberInfo">The type of the member the collection is made of.</typeparam>
        /// <typeparam name="TAttribute">The attribute to look for.</typeparam>
        /// <param name="collection">The collection select from.</param>
        /// <returns>Only the TypeMembers which haev the specified argument defined.</returns>
        public static IEnumerable<KeyValuePair<TMemberInfo, TAttribute>> WithAttribute<TMemberInfo, TAttribute>(
            this IEnumerable<TMemberInfo> collection) where TAttribute : Attribute where TMemberInfo : MemberInfo
        {
            Type attributeType = typeof(TAttribute);
            return from TMemberInfo info in collection
                   let attribute = info.GetCustomAttributes(attributeType, true).SingleOrDefault() as TAttribute
                   where attribute != null
                   select new KeyValuePair<TMemberInfo, TAttribute>(info, attribute);
        }

        /// <summary>
        /// Returns the value of the static field specified by the given name, 
        /// or the default(T) if the field is not found.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="type">The type containing the field.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The value of this field.</returns>
        public static T GetStaticField<T>(Type type, string fieldName)
        {
            var field = type.GetField(fieldName);
            if (field == null)
            {
                return default(T);
            }
            return (T) field.GetValue(null);
        }

        /// <summary>
        /// Verifies that the ClientID/ClientSecret/DeveloperKey is set in the specified class.
        /// </summary>
        /// <param name="type">ClientCredentials.cs class.</param>
        public static void VerifyCredentials(Type type)
        {
            var regex = new Regex("<.+>");

            var errors = (from fieldName in new[] { "ClientID", "ClientSecret", "ApiKey", "BucketPath" }
                          let field = GetStaticField<string>(type, fieldName)
                          where field != null && regex.IsMatch(field)
                          select "- " + fieldName + " is currently not set.").ToList();

            if (errors.Count > 0)
            {
                errors.Insert(0, "Please modify the ClientCredentials.cs:");
                errors.Add("You can find this information on the Google API Console.");
                string msg = String.Join(Environment.NewLine, errors.ToArray());
                CommandLine.WriteError(msg);
                MessageBox.Show(msg, "Please enter your credentials!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
    }
}
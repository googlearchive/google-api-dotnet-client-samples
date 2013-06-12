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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Support for parsing command line flags.
    /// </summary>
    public class CommandLineFlags
    {
        private static readonly Regex ArgumentRegex = new Regex(
           "^-[-]?([^-][^=]*)(=(.*))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parses the specified command line arguments into the specified class.
        /// </summary>
        /// <typeparam name="T">Class where the command line arguments are stored.</typeparam>
        /// <param name="configuration">Class which stores the command line arguments.</param>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Array of unresolved arguments.</returns>
        public static string[] ParseArguments<T>(T configuration, params string[] args)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            List<KeyValuePair<PropertyInfo, ArgumentAttribute>> properties =
                    typeof(T).GetProperties(flags).WithAttribute<PropertyInfo, ArgumentAttribute>().ToList();

            var unresolvedArguments = new List<string>();
            foreach (string arg in args)
            {
                // Parse the argument.
                Match match = ArgumentRegex.Match(arg);
                if (!match.Success) // This is not a typed argument.
                {
                    unresolvedArguments.Add(arg);
                    continue;
                }

                // Extract the argument details.
                bool isShortname = !arg.StartsWith("--");
                string name = match.Groups[1].ToString();
                string value = match.Groups[2].Length > 0 ? match.Groups[2].ToString().Substring(1) : null;

                // Find the argument.
                const StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;
                PropertyInfo property =
                    (from kv in properties
                     where name.Equals(isShortname ? kv.Value.ShortName : kv.Value.Name, ignoreCase)
                     select kv.Key).SingleOrDefault();

                // Check if this is a special argument we should handle.
                if (name == "help")
                {
                    foreach (string line in GenerateCommandLineHelp(configuration))
                    {
                        CommandLine.WriteAction(line);
                    }

                    if (property == null)
                    {
                        // If this isn't handled seperately, close this application.
                        CommandLine.Exit();
                        return null;
                    }
                }
                else if (name == "non-interactive")
                {
                    CommandLine.IsInteractive = false;
                    continue;
                }
                else if (property == null)
                {
                    CommandLine.WriteError("Unknown argument: " + (isShortname ? "-" : "--") + name);
                    continue;
                }

                // Change the property.
                object convertedValue = null;
                if (value == null)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        convertedValue = true;
                    }
                }
                else
                {
                    convertedValue = Convert.ChangeType(value, property.PropertyType);
                }

                if (convertedValue == null)
                {
                    CommandLine.WriteError(
                        string.Format(
                            "Argument '{0}' requires a value of the type '{1}'.", name, property.PropertyType.Name));
                    continue;
                }
                property.SetValue(configuration, convertedValue, null);
            }
            return unresolvedArguments.ToArray();
        }

        /// <summary>
        /// Generates the commandline argument help for a specified type.
        /// </summary>
        /// <typeparam name="T">Configuration.</typeparam>
        public static IEnumerable<string> GenerateCommandLineHelp<T>(T configuration)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            List<KeyValuePair<PropertyInfo, ArgumentAttribute>> properties =
                typeof(T).GetProperties(flags).WithAttribute<PropertyInfo, ArgumentAttribute>().ToList();

            var query = from kv in properties
                        orderby kv.Value.Name
                        // Group the sorted arguments by their category.
                        group kv by kv.Value.Category into g
                        orderby g.Key
                        select g;

            // Go through each category and list all the arguments.
            yield return "Arguments:";
            foreach (var category in query)
            {
                if (!string.IsNullOrEmpty(category.Key))
                {
                    yield return " " + category.Key;
                }

                foreach (KeyValuePair<PropertyInfo, ArgumentAttribute> pair in category)
                {
                    PropertyInfo info = pair.Key;
                    object value = info.GetValue(configuration, null);
                    yield return "   " + FormatCommandLineHelp(pair.Value, info.PropertyType, value);
                }

                yield return "";
            }
        }

        /// <summary>
        /// Generates a single command line help for the specified argument
        /// Example:
        ///     -s, --source=[Something]      Sets the source of ...
        /// </summary>
        private static string FormatCommandLineHelp(ArgumentAttribute attribute, Type propertyType, object value)
        {
            // Generate the list of keywords ("-s, --source").
            var keywords = new List<string>(2);
            if (!string.IsNullOrEmpty(attribute.ShortName))
            {
                keywords.Add("-" + attribute.ShortName);
            }
            keywords.Add("--" + attribute.Name);
            string joinedKeywords = keywords.Aggregate((a, b) => a + ", " + b);

            // Add the assignment-tag, if applicable.
            string assignment = "";
            if (propertyType != typeof(bool))
            {
                assignment = string.Format("=[^1{0}^9]", (value == null) ? ".." : value.ToString());
            }

            // Create the joined left half, and return the full string.
            string left = (joinedKeywords + assignment).PadRight(20);
            return string.Format("^9{0}  ^1{1}", left, attribute.Description);
        }
    }

    /// <summary>
    /// Defines the command line argument structure of a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ArgumentAttribute : Attribute
    {
        private readonly string name;

        /// <summary>
        /// The full name of this command line argument, e.g. "source-directory".
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// The short name of this command line argument, e.g. "src". Optional.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// The description of this command line argument, e.g. "The directory to fetch the data from". Optional.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The category to which this argument belongs, e.g. "I/O flags". Optional.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Defines the command line argument structure of a property.
        /// </summary>
        public ArgumentAttribute(string name)
        {
            this.name = name;
        }
    }
}

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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Contains helper methods for command line operation
    /// </summary>
    public static class CommandLine
    {
        private static readonly Regex ColorRegex = new Regex("{([a-z]+)}", RegexOptions.Compiled);

        /// <summary>
        /// Defines whether this CommandLine can be accessed by an user and is thereby interactive.
        /// True by default.
        /// </summary>
        public static bool IsInteractive { get; set; }

        static CommandLine()
        {
            IsInteractive = true;
        }

        /// <summary>
        /// Creates a new instance of T and fills all public fields by requesting input from the user
        /// </summary>
        /// <typeparam name="T">Class with a default constructor</typeparam>
        /// <returns>Instance of T with filled in public fields</returns>
        public static T CreateClassFromUserinput<T>()
        {
            var type = typeof (T);

            // Create an instance of T
            T settings = Activator.CreateInstance<T>();

            WriteLine("^1 Please enter values for the {0}:", ReflectionUtils.GetDescriptiveName(type));

            // Fill in parameters
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(settings);

                // Let the user input a value
                RequestUserInput(ReflectionUtils.GetDescriptiveName(field), ref value, field.FieldType);

                field.SetValue(settings, value);
            }

            WriteLine();
            return settings;
        }
    
        /// <summary>
        /// Requests an user input for the specified value
        /// </summary>
        /// <param name="name">Name to display</param>
        /// <param name="value">Default value, and target value</param>
        public static void RequestUserInput<T>(string name, ref T value)
        {
            object val = value;
            RequestUserInput(name, ref val, typeof(T));
            value = (T) val;
        }

        /// <summary>
        /// Requests an user input for the specified value, and returns the entered value.
        /// </summary>
        /// <param name="name">Name to display</param>
        public static T RequestUserInput<T>(string name)
        {
            object val = default(T);
            RequestUserInput(name, ref val, typeof(T));
            return (T) val;
        }

        /// <summary>
        /// Requests an user input for the specified value
        /// </summary>
        /// <param name="name">Name to display</param>
        /// <param name="value">Default value, and target value</param>
        /// <param name="valueType">Type of the target value</param>
        private static void RequestUserInput(string name, ref object value, Type valueType)
        {
            do
            {
                if (value != null)
                {
                    Write("   ^1{0} [^8{1}^1]: ^9", name, value);
                }
                else
                {
                    Write("   ^1{0}: ^9", name);
                }

                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    return; // No change required
                }

                try
                {
                    value = Convert.ChangeType(input, valueType);
                    return;
                }
                catch (InvalidCastException)
                {
                    WriteLine(" ^6Please enter a valid value!");
                }
            } while (true); // Run this loop until the user gives a valid input
        }

        /// <summary>
        /// Displays the Google Sample Header
        /// </summary>
        public static void DisplayGoogleSampleHeader(string applicationName)
        {
            applicationName.ThrowIfNull("applicationName");

            Console.BackgroundColor = ConsoleColor.Black;
            try
            {
                Console.Clear();
            } catch (IOException) { } // An exception might occur if the console stream has been redirected.

            WriteLine(@"^3   ___  ^6      ^8      ^3       ^4 _  ^6    ");
            WriteLine(@"^3  / __| ^6 ___  ^8 ___  ^3 __ _  ^4| | ^6 __  ");
            WriteLine(@"^3 | (_ \ ^6/ _ \ ^8/ _ \ ^3/ _` | ^4| | ^6/-_) ");
            WriteLine(@"^3  \___| ^6\___/ ^8\___/ ^3\__, | ^4|_| ^6\___| ");
            WriteLine(@"^3        ^6      ^8      ^3|___/  ^4    ^6    ");
            WriteLine();
            WriteLine("^4 API Samples -- {0}", applicationName);
            WriteLine("^4 Copyright 2011 Google Inc");
            WriteLine();
        }

        /// <summary>
        /// Displays the default "Press any key to exit" message, and waits for an user key input
        /// </summary>
        public static void PressAnyKeyToExit()
        {
            if (IsInteractive)
            {
                WriteLine();
                WriteLine("^8 Press any key to exit^1");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Terminates the application.
        /// </summary>
        public static void Exit()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }

        /// <summary>
        /// Displays the default "Press ENTER to continue" message, and waits for an user key input
        /// </summary>
        public static void PressEnterToContinue()
        {
            if (IsInteractive)
            {
                WriteLine();
                WriteLine("^8 Press ENTER to continue^1");
                while (Console.ReadKey().Key != ConsoleKey.Enter) {}
            }
        }

        /// <summary>
        /// Gives the user a choice of options to choose from
        /// </summary>
        /// <param name="question">The question which should be asked</param>
        /// <param name="choices">All possible choices</param>
        public static void RequestUserChoice(string question, params UserOption[] choices)
        {
            // Validate parameters
            question.ThrowIfNullOrEmpty("question");
            choices.ThrowIfNullOrEmpty("choices");

            // Show the question
            WriteLine(" ^9{0}", question);

            // Display all choices
            int i = 1;

            foreach (UserOption option in choices)
            {
                WriteLine("   ^8{0}.)^9 {1}", i++, option.Name);
            }

            WriteLine();

            // Request user input
            UserOption choice = null;

            do
            {
                Write(" ^1Please pick an option: ^9");
                string input = Console.ReadLine();

                // Check if this is a valid choice
                uint num;

                if (uint.TryParse(input, out num) && num > 0 && choices.Length >= num)
                {
                    // It is a number
                    choice = choices[num - 1];
                }
                else
                {
                    // Check if the user typed in the keyword
                    foreach (UserOption option in choices)
                    {
                        if (String.Equals(option.Name, input, StringComparison.InvariantCultureIgnoreCase))
                        {
                            choice = option;
                            break; // Valid choice
                        }
                    }
                }

                if (choice == null)
                {
                    WriteLine(" ^6Please pick one of the options displayed above!");
                }    
             
            } while (choice == null);

            // Execute the option the user picked
            choice.Target();
        }

        /// <summary>
        /// Gives the user a Yes/No choice and waits for his answer.
        /// </summary>
        public static bool RequestUserChoice(string question)
        {
            question.ThrowIfNull("question");

            // Show the question.
            Write("   ^1{0} [^8{1}^1]: ^9", question, "y/n");

            // Wait for the user input.
            char c;
            do
            {
                c = Console.ReadKey(true).KeyChar;
            } while (c != 'y' && c != 'n');
            WriteLine(c.ToString());

            return c == 'y';
        }

        /// <summary>
        /// Enables the command line exception handling
        /// Prevents the application from just exiting, but tries to display helpful error message instead
        /// </summary>
        public static void EnableExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exception = args.ExceptionObject as Exception;

            // Display the exception
            WriteLine();
            WriteLine(" ^6An error has occured:");

            WriteLine("    ^6{0}", exception == null ? "<unknown error>" : exception.Message);

            // Display stacktrace
            if (IsInteractive)
            {
                WriteLine();
                WriteLine("^8 Press any key to display the stacktrace");
                Console.ReadKey();
            }
            WriteLine();
            WriteLine(" ^1{0}", exception);

            // Close the application
            PressAnyKeyToExit();
            Environment.Exit(-1);
        }

        /// <summary>
        /// Writes the specified text to the console
        /// Applies special color filters (^0, ^1, ...)
        /// </summary>
        public static void Write(string format, params object[] values)
        {
            string text = String.Format(format, values);
            Console.ForegroundColor = ConsoleColor.Gray;

            // Replace ^1, ... color tags.
            while (text.Contains("^"))
            {
                int index = text.IndexOf("^");

                // Check if a number follows the index
                if (index+1 < text.Length && Char.IsDigit(text[index+1]))
                {
                    // Yes - it is a color notation
                    InternalWrite(text.Substring(0, index)); // Pre-Colornotation text
                    Console.ForegroundColor = (ConsoleColor) (text[index + 1] - '0' + 6);
                    text = text.Substring(index + 2); // Skip the two-char notation
                }
                else
                {
                    // Skip ahead
                    InternalWrite(text.Substring(0, index));
                    text = text.Substring(index + 1);
                }
            }

            // Write the remaining text
            InternalWrite(text);
        }

        private static void InternalWrite(string text)
        {
            // Check for color tags.
            Match match;
            while ((match = ColorRegex.Match(text)).Success)
            {
                // Write the text before the tag.
                Console.Write(text.Substring(0, match.Index));

                // Change the color
                Console.ForegroundColor = GetColor(match.Groups[1].ToString());
                text = text.Substring(match.Index + match.Length);
            }

            // Write the remaining text.
            Console.Write(text);
        }

        private static ConsoleColor GetColor(string id)
        {
            return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), id, true);
        }

        /// <summary>
        /// Writes the specified text to the console
        /// Applies special color filters (^0, ^1, ...)
        /// </summary>
        public static void WriteLine(string format, params object[] values)
        {
            Write(format+Environment.NewLine, values);
        }

        /// <summary>
        /// Writes an empty line into the console stream
        /// </summary>
        public static void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Writes a result into the console stream.
        /// </summary>
        public static void WriteResult(string name, object value)
        {
            if (value == null)
            {
                value = "<null>";
            }
            string strValue = value.ToString();
            if (strValue.Length == 0)
            {
                strValue = "<empty>";
            }

            WriteLine("   ^4{0}: ^9{1}", name, strValue);
        }

        /// <summary>
        /// Writes an action statement into the console stream.
        /// </summary>
        public static void WriteAction(string action)
        {
            WriteLine(" ^8{0}", action);
        }

        /// <summary>
        /// Writes an error into the console stream.
        /// </summary>
        public static void WriteError(string error, params object[] values)
        {
            WriteLine(" ^6"+error, values);
        }
    }
}

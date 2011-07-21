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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using Google.Apis.Discovery;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Samples.Helper;
using Google.Apis.Tools.CodeGen;
using DiscoveryService = Google.Apis.Discovery.v1.DiscoveryService;

namespace Google.Apis.Samples.CmdServiceGenerator
{
    /// <summary>
    /// This example is a command line service generator, which uses discovery and the CodeGen library
    /// to create a strongly typed binding for the specified service. You can either use this example,
    /// or use the pregenerated service files found on the Google APIs page.
    /// </summary>
    internal class Program
    {
        private class CommandLineArguments
        {
            [CommandLine.Argument("all", ShortName = "a",
                Description = "Generate all services listed in Google Discovery")]
            public bool GenerateAllServices { get; set; }

            [CommandLine.Argument("compile", ShortName = "c",
                Description = "Compiles the source code into a .dll library")]
            public bool CompileIntoLibrary { get; set; }

            [CommandLine.Argument("discovery", Category = "Discovery settings", ShortName = "s",
                Description = "Specifices a discovery document to use")]
            public string DiscoveryDocument { get; set; }

            [CommandLine.Argument("discovery-version", Category = "Discovery settings", ShortName = "v",
                Description = "Defines the discovery version to use (1/0.3)")]
            public string DiscoveryVersion { get; set; }

            /// <summary>
            /// Returns the discovery version to use
            /// </summary>
            public DiscoveryVersion GetDiscoveryVersion()
            {
                switch (DiscoveryVersion)
                {
                    default:
                    case "1":
                    case "1.0":
                        return Discovery.DiscoveryVersion.Version_1_0;

                    case "0.3":
                        return Discovery.DiscoveryVersion.Version_0_3;
                }
            }
        }
        
        /// <summary>
        /// User input for this example
        /// </summary>
        [Description("service")]
        private class ServiceDescription
        {
            [Description("service name")] 
            public string ServiceName = "discovery";
            [Description("service version")]
            public string ServiceVersion = "v1";
        }

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Optional: [serviceName serviceVersion]</param>
        static void Main(string[] args)
        {
            CommandLine.EnableExceptionHandling();
            CommandLine.DisplayGoogleSampleHeader("Service Generator");
            var cmdArgs = new CommandLineArguments();
            string[] unparsed = CommandLine.ParseArguments(cmdArgs, args);

            // Parse command line args
            if (cmdArgs.GenerateAllServices)
            {
                GenerateAllServices(cmdArgs);
            }
            else if (!string.IsNullOrEmpty(cmdArgs.DiscoveryDocument))
            {
                GenerateService(new Uri(cmdArgs.DiscoveryDocument), cmdArgs);
            }
            else if (unparsed.Length >= 2)
            {
                var desc = new ServiceDescription { ServiceName = unparsed[0], ServiceVersion = unparsed[1] };
                GenerateService(desc, cmdArgs);
            }
            else
            {
                var desc = CommandLine.CreateClassFromUserinput<ServiceDescription>();
                GenerateService(desc, cmdArgs);
            }

            // ..and we are done!
            CommandLine.PressAnyKeyToExit();
        }

        static void GenerateService(ServiceDescription description, CommandLineArguments cmdArgs)
        {
            string uri = string.Format(
                GoogleServiceGenerator.GoogleDiscoveryURL, description.ServiceName, description.ServiceVersion);
            GenerateService(new Uri(uri), cmdArgs);
        }

        static void GenerateService(Uri url, CommandLineArguments cmdArgs)
        {
            // Create the output directory if it does not exist yet.
            if (!Directory.Exists("Generated"))
            {
                Directory.CreateDirectory("Generated");
            }

            // Discover the service.
            IDiscoveryDevice src = url.IsFile
                                       ? (IDiscoveryDevice)
                                         new StreamDiscoveryDevice
                                             {
                                                 DiscoveryStream =
                                                     File.Open(url.ToString().Replace("file:///", ""), FileMode.Open)
                                             }
                                       : new CachedWebDiscoveryDevice(url);
            var discovery = new Discovery.DiscoveryService(src);
            var service = discovery.GetService("v1", cmdArgs.GetDiscoveryVersion());
            
            // Generate the formal names based upon the discovery data.
            string name = service.Name;
            string version = service.Version;

            string formalServiceName = GeneratorUtils.UpperFirstLetter(name);
            string fileName = string.Format("Generated/{0}.cs", formalServiceName);
            string libfileName = string.Format("Generated/{0}.dll", formalServiceName);
            string serviceNamespace = string.Format("Google.Apis.{0}.{1}", formalServiceName, version);

            // Produce the code.
            var generator = new GoogleServiceGenerator(service, serviceNamespace);
            CodeCompileUnit generatedCode = generator.GenerateCode();
            var provider = CodeDomProvider.CreateProvider("CSharp");

            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "  ");

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(generatedCode, tw, new CodeGeneratorOptions());

                // Close the output file.
                tw.Close();
            }

            CommandLine.WriteLine("^9 Service generated in ^4" + fileName + "^9!");

            if (cmdArgs.CompileIntoLibrary)
            {
                CompileIntoLibrary(generatedCode, libfileName);
            }
        }

        static void CompileIntoLibrary(CodeCompileUnit code, string targetFile)
        {
            var cp = new CompilerParameters();
            cp.OutputAssembly = targetFile;
            cp.GenerateExecutable = false;
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("log4net.dll");

            foreach (Type type in new[] { typeof(Newtonsoft.Json.JsonConvert), typeof(GoogleApiException) })
            {
                cp.ReferencedAssemblies.Add(type.Assembly.CodeBase.Replace("file:///", ""));
            }

            var provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerResults results = provider.CompileAssemblyFromDom(cp, code);
            
            if (results.Errors.HasErrors || results.CompiledAssembly == null)
            {
                CommandLine.WriteError("Compilation failed!");
                foreach (CompilerError error in results.Errors)
                {
                    CommandLine.WriteError("{0}:{1} - {2}", error.FileName, error.Line, error.ErrorText);
                }
            }
            else
            {
                CommandLine.WriteLine("^9 Library generated in ^4" + targetFile + "^9!");
            }
        }

        static void GenerateAllServices(CommandLineArguments cmdArgs)
        {
            var response = new DiscoveryService().Apis.List().Fetch();

            if (response.Items == null)
            {
                CommandLine.WriteError("No services found!");
                return;
            }

            foreach (DirectoryList.ItemsData service in response.Items)
            {
                GenerateService(
                    new ServiceDescription() { ServiceName = service.Name, ServiceVersion = service.Version }, cmdArgs);
            }
        }
    }
}

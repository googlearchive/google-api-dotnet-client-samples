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
using System.Net;
using System.Reflection;
using Google.Apis.Authentication;
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
        private static DiscoveryService _service;

        private static DiscoveryService Service
        {
            get
            {
                if (_service == null)
                {
                    var auth = new DelegateAuthenticator(wr => wr.Headers["X-User-IP"] = "0.0.0.0");
                    _service = new DiscoveryService(auth);
                }
                return _service;
            }
        }

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
                CommandLine.PressAnyKeyToExit();
            }

            // ..and we are done!
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
            const string DIR = "Services";
            if (!Directory.Exists(DIR))
            {
                Directory.CreateDirectory(DIR);
            }

            // Discover the service.
            CommandLine.WriteAction("Fetching " + url);
            IDiscoveryDevice src = url.IsFile
                                       ? (IDiscoveryDevice)
                                         new StreamDiscoveryDevice
                                             {
                                                 DiscoveryStream =
                                                     File.Open(url.ToString().Replace("file:///", ""), FileMode.Open)
                                             }
                                       : new StringDiscoveryDevice { Document = FetchDocument(url) };
            var discovery = new Discovery.DiscoveryService(src);
            var service = discovery.GetService(cmdArgs.GetDiscoveryVersion());
            
            // Generate the formal names based upon the discovery data.
            string name = service.Name;
            string version = service.Version;

            string formalServiceName = GeneratorUtils.UpperFirstLetter(name);
            string baseFileName = string.Format("{2}/{0}.{1}", formalServiceName, version, DIR);
            string fileName = baseFileName + ".cs";
            string libfileName = baseFileName + ".dll";
            string serviceNamespace = string.Format(
                "Google.Apis.{0}.{1}", formalServiceName, version.Replace('.', '_'));

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
                CompileIntoLibrary(service, generatedCode, libfileName);
            }
        }

        static void CompileIntoLibrary(IService service, CodeCompileUnit code, string targetFile)
        {
            string xmlDocFile = targetFile.Replace(".dll", ".xml");

            // Set the AssemblyInfo
            AddAssemblyInfo<AssemblyTitleAttribute>(code, service.Title);
            AddAssemblyInfo<AssemblyCompanyAttribute>(code, "Google Inc");
            AddAssemblyInfo<AssemblyProductAttribute>(code, service.Id);
            AddAssemblyInfo<AssemblyVersionAttribute>(code, typeof(IService).Assembly.GetName().Version.ToString());
            AddAssemblyInfo<AssemblyCopyrightAttribute>(code, "© "+DateTime.UtcNow.Year+" Google Inc");

            // Set up the compiler.
            var cp = new CompilerParameters();
            cp.OutputAssembly = targetFile;
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("log4net.dll");
            cp.IncludeDebugInformation = true;
            cp.CompilerOptions = "/doc:" + xmlDocFile;

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
                CommandLine.WriteLine("^9 XML Doc generated in ^4" + xmlDocFile + "^9!");
            }
        }

        static void GenerateAllServices(CommandLineArguments cmdArgs)
        {
            var response = Service.Apis.List().Fetch();

            if (response.Items == null)
            {
                CommandLine.WriteError("No services found!");
                return;
            }

            foreach (DirectoryList.ItemsData service in response.Items)
            {
                GenerateService(
                    new ServiceDescription { ServiceName = service.Name, ServiceVersion = service.Version }, cmdArgs);
                CommandLine.WriteLine();
            }
        }

        static string FetchDocument(Uri uri)
        {
            var webClient = new WebClient();
            webClient.Headers["X-User-IP"] = "0.0.0.0";
            return webClient.DownloadString(uri);
        }

        static void AddAssemblyInfo<T>(CodeCompileUnit code, string value)
        {
            var attrib = new CodeAttributeDeclaration(new CodeTypeReference(typeof(T)));
            attrib.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(value)));
            code.AssemblyCustomAttributes.Add(attrib);
        }
    }
}

// ***********************************************************************
// Copyright (c) 2014-2020 Charlie Poole, Terje Sandstrom
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace NUnit.VisualStudio.TestAdapter
{
    public interface IAdapterSettings
    {
        int MaxCpuCount { get; }
        string ResultsDirectory { get; }
        string TargetPlatform { get; }
        string TargetFrameworkVersion { get; }
        string TestAdapterPaths { get; }
        bool CollectSourceInformation { get; }
        IDictionary<string, string> TestProperties { get; }
        string InternalTraceLevel { get; }
        string WorkDirectory { get; }
        string Where { get; }
        int DefaultTimeout { get; }
        int NumberOfTestWorkers { get; }
        bool ShadowCopyFiles { get; }
        int Verbosity { get; }
        bool UseVsKeepEngineRunning { get; }
        string BasePath { get; }
        string PrivateBinPath { get; }
        int? RandomSeed { get; }
        bool RandomSeedSpecified { get; }
        bool InProcDataCollectorsAvailable { get; }
        // ReSharper disable once UnusedMemberInSuper.Global
        bool CollectDataForEachTestSeparately { get; }  // Used implicitly by MS
        bool SynchronousEvents { get; }
        string DomainUsage { get; }
        bool DumpXmlTestDiscovery { get; }
        bool DumpXmlTestResults { get; }

        bool PreFilter { get; }

        /// <summary>
        ///  Syntax documentation <see cref="https://github.com/nunit/docs/wiki/Template-Based-Test-Naming"/>.
        /// </summary>
        string DefaultTestNamePattern { get; }

        VsTestCategoryType VsTestCategoryType { get; }
        string TestOutputXml { get; }
        bool UseTestOutputXml { get; }

        /// <summary>
        /// True if test run is triggered in an IDE/Editor context.
        /// </summary>
        bool DesignMode { get; }

        /// <summary>
        /// If true, an adapter shouldn't create appdomains to run tests.
        /// </summary>
        bool DisableAppDomain { get; }

        /// <summary>
        /// If true, an adapter should disable any test case parallelization.
        /// </summary>
        bool DisableParallelization { get; }

        bool ShowInternalProperties { get; }

        bool UseParentFQNForParametrizedTests { get; }

        bool UseNUnitIdforTestCaseId { get; }

        int ConsoleOut { get; }
        bool StopOnError { get; }
        TestOutcome MapWarningTo { get; }
        bool UseTestNameInConsoleOutput { get; }
        bool FreakMode { get; }
        DisplayNameOptions DisplayName { get; }
        char FullnameSeparator { get; }
        bool SkipNonTestAssemblies { get; }

        void Load(IDiscoveryContext context);
        void Load(string settingsXml);
        void SaveRandomSeed(string dirname);
        void RestoreRandomSeed(string dirname);
    }

    public enum VsTestCategoryType
    {
        NUnit,
        MsTest
    }

    public enum DisplayNameOptions
    {
        Name,
        FullName,
        FullNameSep
    }

    public class AdapterSettings : IAdapterSettings
    {
        private const string RANDOM_SEED_FILE = "nunit_random_seed.tmp";
        private readonly TestLogger _logger;

        #region Constructor

        public AdapterSettings(TestLogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Properties - General

        public int MaxCpuCount { get; private set; }

        public string ResultsDirectory { get; private set; }

        public string TargetPlatform { get; private set; }

        public string TargetFrameworkVersion { get; private set; }

        public string TestAdapterPaths { get; private set; }

        /// <summary>
        /// If false, an adapter need not parse symbols to provide test case file, line number.
        /// </summary>
        public bool CollectSourceInformation { get; private set; }

        /// <summary>
        /// If true, an adapter shouldn't create appdomains to run tests.
        /// </summary>
        public bool DisableAppDomain { get; private set; }

        /// <summary>
        /// If true, an adapter should disable any test case parallelization.
        /// </summary>
        public bool DisableParallelization { get; private set; }

        /// <summary>
        /// True if test run is triggered in an IDE/Editor context.
        /// </summary>
        public bool DesignMode { get; private set; }

        #endregion

        #region Properties - TestRunParameters

        public IDictionary<string, string> TestProperties { get; private set; }

        #endregion

        #region Properties - NUnit Specific

        public string InternalTraceLevel { get; private set; }

        public string WorkDirectory { get; private set; }
        public string Where { get; private set; }
        public string TestOutputXml { get; private set; }
        public bool UseTestOutputXml => !string.IsNullOrEmpty(TestOutputXml);
        public int DefaultTimeout { get; private set; }

        public int NumberOfTestWorkers { get; private set; }

        public bool ShadowCopyFiles { get; private set; }

        public int Verbosity { get; private set; }

        public bool UseVsKeepEngineRunning { get; private set; }

        public string BasePath { get; private set; }

        public string PrivateBinPath { get; private set; }

        public int? RandomSeed { get; private set; }
        public bool RandomSeedSpecified { get; private set; }

        public bool CollectDataForEachTestSeparately { get; private set; }

        public bool InProcDataCollectorsAvailable { get; private set; }

        public bool SynchronousEvents { get; private set; }

        public string DomainUsage { get; private set; }

        public bool ShowInternalProperties { get; private set; }
        public bool UseParentFQNForParametrizedTests { get; private set; }  // Default is false.  True can fix certain test name patterns, but may have side effects.
        public bool UseNUnitIdforTestCaseId { get; private set; }  // default is false.
        public int ConsoleOut { get; private set; }
        public bool StopOnError { get; private set; }

        public bool SkipNonTestAssemblies { get; private set; }


        public VsTestCategoryType VsTestCategoryType { get; private set; } = VsTestCategoryType.NUnit;

        /// <summary>
        ///  Syntax documentation <see cref="https://github.com/nunit/docs/wiki/Template-Based-Test-Naming"/>.
        /// </summary>
        public string DefaultTestNamePattern { get; set; }

        public bool PreFilter { get; private set; }

        public TestOutcome MapWarningTo { get; private set; }

        public bool UseTestNameInConsoleOutput { get; private set; }

        public DisplayNameOptions DisplayName { get; private set; } = DisplayNameOptions.Name;

        public char FullnameSeparator { get; private set; } = ':';




        #region  NUnit Diagnostic properties
        public bool DumpXmlTestDiscovery { get; private set; }

        public bool DumpXmlTestResults { get; private set; }

        public bool FreakMode { get; private set; }

        #endregion



        #endregion

        #region Public Methods

        public void Load(IDiscoveryContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Load called with null context");

            Load(context.RunSettings?.SettingsXml);
        }

        public void Load(string settingsXml)
        {
            if (string.IsNullOrEmpty(settingsXml))
                settingsXml = "<RunSettings />";

            // Visual Studio already gives a good error message if the .runsettings
            // file is poorly formed, so we don't need to do anything more.
            var doc = new XmlDocument();
            doc.LoadXml(settingsXml);

            var nunitNode = doc.SelectSingleNode("RunSettings/NUnit");
            Verbosity = GetInnerTextAsInt(nunitNode, nameof(Verbosity), 0);
            _logger.Verbosity = Verbosity;

            var runConfiguration = doc.SelectSingleNode("RunSettings/RunConfiguration");
            MaxCpuCount = GetInnerTextAsInt(runConfiguration, nameof(MaxCpuCount), -1);
            ResultsDirectory = GetInnerTextWithLog(runConfiguration, nameof(ResultsDirectory));
            TargetPlatform = GetInnerTextWithLog(runConfiguration, nameof(TargetPlatform));
            TargetFrameworkVersion = GetInnerTextWithLog(runConfiguration, nameof(TargetFrameworkVersion));
            TestAdapterPaths = GetInnerTextWithLog(runConfiguration, nameof(TestAdapterPaths));
            CollectSourceInformation = GetInnerTextAsBool(runConfiguration, nameof(CollectSourceInformation), true);
            DisableAppDomain = GetInnerTextAsBool(runConfiguration, nameof(DisableAppDomain), false);
            DisableParallelization = GetInnerTextAsBool(runConfiguration, nameof(DisableParallelization), false);
            DesignMode = GetInnerTextAsBool(runConfiguration, nameof(DesignMode), false);
            CollectDataForEachTestSeparately =
                GetInnerTextAsBool(runConfiguration, nameof(CollectDataForEachTestSeparately), false);

            TestProperties = new Dictionary<string, string>();
            UpdateTestProperties();

            // NUnit settings
            InternalTraceLevel = GetInnerTextWithLog(nunitNode, nameof(InternalTraceLevel), "Off", "Error", "Warning",
                "Info", "Verbose", "Debug");
            WorkDirectory = GetInnerTextWithLog(nunitNode, nameof(WorkDirectory));
            Where = GetInnerTextWithLog(nunitNode, nameof(Where));
            DefaultTimeout = GetInnerTextAsInt(nunitNode, nameof(DefaultTimeout), 0);
            NumberOfTestWorkers = GetInnerTextAsInt(nunitNode, nameof(NumberOfTestWorkers), -1);
            ShadowCopyFiles = GetInnerTextAsBool(nunitNode, nameof(ShadowCopyFiles), false);
            UseVsKeepEngineRunning = GetInnerTextAsBool(nunitNode, nameof(UseVsKeepEngineRunning), false);
            BasePath = GetInnerTextWithLog(nunitNode, nameof(BasePath));
            PrivateBinPath = GetInnerTextWithLog(nunitNode, nameof(PrivateBinPath));
            TestOutputXml = GetInnerTextWithLog(nunitNode, nameof(TestOutputXml));
            RandomSeed = GetInnerTextAsNullableInt(nunitNode, nameof(RandomSeed));
            RandomSeedSpecified = RandomSeed.HasValue;
            if (!RandomSeedSpecified)
                RandomSeed = new Random().Next();
            DefaultTestNamePattern = GetInnerTextWithLog(nunitNode, nameof(DefaultTestNamePattern));
            ShowInternalProperties = GetInnerTextAsBool(nunitNode, nameof(ShowInternalProperties), false);
            UseParentFQNForParametrizedTests = GetInnerTextAsBool(nunitNode, nameof(UseParentFQNForParametrizedTests), false);
            UseNUnitIdforTestCaseId = GetInnerTextAsBool(nunitNode, nameof(UseNUnitIdforTestCaseId), false);
            ConsoleOut = GetInnerTextAsInt(nunitNode, nameof(ConsoleOut), 1);  // 0 no output to console, 1 : output to console
            StopOnError = GetInnerTextAsBool(nunitNode, nameof(StopOnError), false);

            // Engine settings
            SkipNonTestAssemblies = GetInnerTextAsBool(nunitNode, nameof(SkipNonTestAssemblies), true);


            // Adapter Diagnostics
            DumpXmlTestDiscovery = GetInnerTextAsBool(nunitNode, nameof(DumpXmlTestDiscovery), false);
            DumpXmlTestResults = GetInnerTextAsBool(nunitNode, nameof(DumpXmlTestResults), false);
            FreakMode = GetInnerTextAsBool(nunitNode, nameof(FreakMode), false);
            // End Diagnostics

            // Adapter Display Options
            MapDisplayName(GetInnerText(nunitNode, nameof(DisplayName), Verbosity > 0));
            FullnameSeparator = GetInnerText(nunitNode, nameof(FullnameSeparator), Verbosity > 0)?[0] ?? ':';

            // EndDisplay



            PreFilter = GetInnerTextAsBool(nunitNode, nameof(PreFilter), false);
            MapTestCategory(GetInnerText(nunitNode, nameof(VsTestCategoryType), Verbosity > 0));
            MapWarningTo = MapWarningOutcome(GetInnerText(nunitNode, nameof(MapWarningTo), Verbosity > 0));
            UseTestNameInConsoleOutput = GetInnerTextAsBool(nunitNode, nameof(UseTestNameInConsoleOutput), false);
            var inProcDataCollectorNode =
                doc.SelectSingleNode("RunSettings/InProcDataCollectionRunSettings/InProcDataCollectors");
            InProcDataCollectorsAvailable = inProcDataCollectorNode != null &&
                                            inProcDataCollectorNode.SelectNodes("InProcDataCollector")?.Count > 0;

            // Older versions of VS do not pass the CollectDataForEachTestSeparately configuration together with the LiveUnitTesting collector.
            // However, the adapter is expected to run in CollectDataForEachTestSeparately mode.
            // As a result for backwards compatibility reasons enable CollectDataForEachTestSeparately mode whenever LiveUnitTesting collector is being used.
            var hasLiveUnitTestingDataCollector =
                inProcDataCollectorNode?.SelectSingleNode(
                    "InProcDataCollector[@uri='InProcDataCollector://Microsoft/LiveUnitTesting/1.0']") != null;

            // TestPlatform can opt-in to run tests one at a time so that the InProcDataCollectors can collect the data for each one of them separately.
            // In that case, we need to ensure that tests do not run in parallel and the test started/test ended events are sent synchronously.
            if (CollectDataForEachTestSeparately || hasLiveUnitTestingDataCollector)
            {
                NumberOfTestWorkers = 0;
                SynchronousEvents = true;
                if (Verbosity >= 4)
                {
                    if (!InProcDataCollectorsAvailable)
                    {
                        _logger.Info(
                            "CollectDataForEachTestSeparately is set, which is used to make InProcDataCollectors collect data for each test separately. No InProcDataCollectors can be found, thus the tests will run slower unnecessarily.");
                    }
                }
            }

            // If DisableAppDomain settings is passed from the testplatform, set the DomainUsage to None.
            if (DisableAppDomain)
            {
                DomainUsage = "None";
            }

            // Update NumberOfTestWorkers based on the DisableParallelization and NumberOfTestWorkers from runsettings.
            UpdateNumberOfTestWorkers();


            string ValidatedPath(string path, string purpose)
            {
                try
                {
                    if (string.IsNullOrEmpty(WorkDirectory))
                    {
                        return Path.GetFullPath(path);
                    }

                    if (Path.IsPathRooted(path))
                    {
                        return Path.GetFullPath(path);
                    }
                    return Path.GetFullPath(Path.Combine(WorkDirectory, path));
                }
                catch (Exception)
                {
                    _logger.Error($"   Invalid path for {purpose}: {path}");
                    throw;
                }
            }

            void UpdateTestProperties()
            {
                foreach (XmlNode node in doc.SelectNodes("RunSettings/TestRunParameters/Parameter"))
                {
                    var key = node.GetAttribute("name");
                    var value = node.GetAttribute("value");
                    if (key != null && value != null)
                        TestProperties.Add(key, value);
                }
            }
        }

        private void MapTestCategory(string vsTestCategoryType)
        {
            if (vsTestCategoryType == null)
                return;
            var ok = TryParse.EnumTryParse(vsTestCategoryType, out VsTestCategoryType result);
            if (ok)
                VsTestCategoryType = result;
            else
                _logger.Warning($"Invalid value ({vsTestCategoryType}) for VsTestCategoryType, should be either NUnit or MsTest");
        }

        private void MapDisplayName(string displaynameoptions)
        {
            if (displaynameoptions == null)
                return;
            var ok = TryParse.EnumTryParse(displaynameoptions, out DisplayNameOptions result);
            if (ok)
                DisplayName = result;
            else
                _logger.Warning($"Invalid value ({displaynameoptions}) for DisplayNameOptions, should be either Name, Fullname or FullnameSep");
        }


        public void SaveRandomSeed(string dirname)
        {
            try
            {
                var path = Path.Combine(dirname, RANDOM_SEED_FILE);
                File.WriteAllText(path, RandomSeed.Value.ToString());
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to save random seed.", ex);
            }
        }

        public void RestoreRandomSeed(string dirname)
        {
            var fullPath = Path.Combine(dirname, RANDOM_SEED_FILE);
            if (!File.Exists(fullPath))
                return;
            try
            {
                var value = File.ReadAllText(fullPath);
                RandomSeed = int.Parse(value);
            }
            catch (Exception ex)
            {
                _logger.Warning("Unable to restore random seed.", ex);
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateNumberOfTestWorkers()
        {
            // Overriding the NumberOfTestWorkers if DisableParallelization is true.
            if (DisableParallelization && NumberOfTestWorkers < 0)
            {
                NumberOfTestWorkers = 0;
            }
            else if (DisableParallelization && NumberOfTestWorkers > 0)
            {
                if (_logger.Verbosity > 0)
                {
                    _logger.Warning(
                        $"DisableParallelization:{DisableParallelization} & NumberOfTestWorkers:{NumberOfTestWorkers} are conflicting settings, hence not running in parallel");
                }
                NumberOfTestWorkers = 0;
            }
        }

        private string GetInnerTextWithLog(XmlNode startNode, string xpath, params string[] validValues)
        {
            return GetInnerText(startNode, xpath, true, validValues);
        }


        private string GetInnerText(XmlNode startNode, string xpath, bool log, params string[] validValues)
        {
            string val = null;
            var targetNode = startNode?.SelectSingleNode(xpath);
            if (targetNode != null)
            {
                val = targetNode.InnerText;

                if (validValues != null && validValues.Length > 0)
                {
                    foreach (string valid in validValues)
                    {
                        if (string.Compare(valid, val, StringComparison.OrdinalIgnoreCase) == 0)
                            return valid;
                    }

                    throw new ArgumentException($"Invalid value {val} passed for element {xpath}.");
                }
            }
            if (log)
                Log(xpath, val);

            return val;
        }

        private int GetInnerTextAsInt(XmlNode startNode, string xpath, int defaultValue)
        {
            var temp = GetInnerTextAsNullableInt(startNode, xpath, false);
            int res = defaultValue;
            if (temp != null)
                res = temp.Value;
            Log(xpath, res);
            return res;
        }

        private int? GetInnerTextAsNullableInt(XmlNode startNode, string xpath, bool log = true)
        {
            string temp = GetInnerText(startNode, xpath, log);
            int? res = null;
            if (!string.IsNullOrEmpty(temp))
                res = int.Parse(temp);
            if (log)
                Log(xpath, res);
            return res;
        }

        private bool GetInnerTextAsBool(XmlNode startNode, string xpath, bool defaultValue)
        {
            string temp = GetInnerText(startNode, xpath, false);
            bool res = defaultValue;
            if (!string.IsNullOrEmpty(temp))
                res = bool.Parse(temp);
            Log(xpath, res);
            return res;
        }

        private void Log<T>(string xpath, T res)
        {
            if (Verbosity >= 4)
            {
                _logger.Info($"Setting: {xpath} = {res}");
            }
        }

        public TestOutcome MapWarningOutcome(string outcome)
        {
            if (outcome == null)
                return TestOutcome.Skipped;

            bool ok = TryParse.EnumTryParse(outcome, out TestOutcome testoutcome);

            if (!ok)
            {
                _logger.Warning(
                    $"Invalid value ({outcome}) for MapWarningTo, should be either Skipped,Failed,Passed or None");
                return TestOutcome.Skipped;
            }
            return testoutcome;
        }
        #endregion
    }
}

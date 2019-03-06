﻿namespace ScriptBuilderTask.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using NServiceBus.Metrics.PerformanceCounters;
    using NUnit.Framework;
    using Particular.Approvals;

    [TestFixture]
    public class PowershellCodeGenerationTests
    {
        InnerTask task;
        string tempPath;

        [SetUp]
        public void SetUp()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            tempPath = testDirectory;
            var assemblyPath = Path.Combine(testDirectory, "ScriptBuilderTask.Tests.dll");
            var metricsAssemblyPath = Path.Combine(testDirectory, "NServiceBus.Metrics.dll");
            var intermediatePath = Path.Combine(tempPath, "IntermediatePath");
            var promotePath = Path.Combine(tempPath, "PromotePath");
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(intermediatePath);

            Action<string, string> logError = (error, s1) => { throw new Exception(error); };
            task = new InnerTask(assemblyPath, metricsAssemblyPath, intermediatePath, "TheProjectDir", promotePath, logError);
        }

        [Test]
        public void Generates()
        {
            task.Execute();

            var powershell = Directory.EnumerateFiles(tempPath, "*.ps1", SearchOption.AllDirectories).Single();
            var content = File.ReadAllText(powershell);

            Approver.Verify(content);
        }
    }
}
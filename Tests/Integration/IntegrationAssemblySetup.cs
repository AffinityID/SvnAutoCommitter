using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace SvnAutoCommitter.Tests.Integration {
    [SetUpFixture]
    public class IntegrationAssemblySetup {
        [SetUp]
        public void BeforeAllTestsInAssembly() {
            SetupTestLogging();
        }

        private static void SetupTestLogging() {
            var hierarchy = (Hierarchy) LogManager.GetRepository();

            var patternLayout = new PatternLayout {
                ConversionPattern = "%date [%thread] %-5level %logger{1} - %message%newline"
            };
            patternLayout.ActivateOptions();

            var console = new ConsoleAppender {
                Layout = patternLayout
            };
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);
            
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }
    }
}

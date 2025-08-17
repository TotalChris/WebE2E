using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebE2E
{

    public record TestConfig(BrowserType browser, string startUrl)
    {
        public override string ToString()
        {
            var environment = startUrl switch
            {
                "http://localhost:5173" => "Local",
                "https://example.hosting.app" => "Stage",
                "https://example.com" => "Prod",
                _ => "Unknown"
            };

            return $"{browser} - {environment}";
        }
    }

    [TestFixture]
    public class ProductTestFixture : ProductComponent
    {
        public static IPlaywright Playwright { get; private set; } = null!;
        public static IBrowser Browser { get; private set; } = null!;
        public static IBrowserContext Context { get; private set; } = null!;

        [SetUp]
        public async Task TestSetup()
        {
            (BrowserType browser, string startUrl) = GetConfigFromContext();

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            
            Browser = await LaunchBrowserAsync(browser);

            Context = await Browser.NewContextAsync(new() { BaseURL = startUrl });

            Page = await Context.NewPageAsync();

            await Page.GotoAsync(startUrl);
        }

        private static TestConfig GetConfigFromContext()
        {
            var config = TestContext.CurrentContext.Test.Properties.Get("Config");
            return config as TestConfig ?? throw new InvalidOperationException("Test configuration could not be resolved");
        }

        private async Task<IBrowser> LaunchBrowserAsync(BrowserType browserType)
        {
            return browserType switch
            {
                BrowserType.Chromium => await Playwright.Chromium.LaunchAsync(new() { Headless = !Debugger.IsAttached }),
                BrowserType.Firefox => await Playwright.Firefox.LaunchAsync(new() { Headless = !Debugger.IsAttached }),
                BrowserType.Webkit => await Playwright.Webkit.LaunchAsync(new() { Headless = !Debugger.IsAttached }),
                _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
            };
        }

        private string CreateScreenshotPath(BrowserType browser, string startUrl, string testName)
        {
            var environment = startUrl switch
            {
                "http://localhost:5173" => "Local",
                "https://example.hosting.app" => "Stage",
                "https://example.com" => "Prod",
                _ => "Unknown"
            };

            var rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            rootPath = Path.Combine(rootPath, $"TestFailureScreenshots/{testName}/{environment}/");
            if(!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            return Path.Combine(rootPath, $"{browser}.png");
        }

        [TearDown]
        public async Task TestTeardown()
        {
            (BrowserType browser, string startUrl) = GetConfigFromContext();
            var testName = TestContext.CurrentContext.Test.MethodName;

            if(TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                await Page.ScreenshotAsync(new() { FullPage = true, Path = CreateScreenshotPath(browser, startUrl, testName) });
            }

            await Page.CloseAsync();
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }

    public class ProductComponent
    {
        public IPage Page { get; protected set; } = null!;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ProductTest: NUnitAttribute, ITestBuilder, IImplyFixture
    {

        private readonly NUnitTestCaseBuilder builder = new();

        public static readonly TestConfig[] Configs =
        {
            new(BrowserType.Chromium, "http://localhost:5173"),
            new(BrowserType.Chromium, "https://example.hosting.app"),
            new(BrowserType.Chromium, "https://example.com"),
            new(BrowserType.Firefox, "http://localhost:5173"),
            new(BrowserType.Firefox, "https://example.hosting.app"),
            new(BrowserType.Firefox, "https://example.com"),
            // new(BrowserType.Webkit, "http://localhost:5173"), Disabled due to Webkit cookie policy on localhost
            new(BrowserType.Webkit, "https://example.hosting.app"),
            new(BrowserType.Webkit, "https://example.com"),
        };

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            foreach(var config in Configs)
            {
                var parameters = new TestCaseParameters(new[] { config });
                parameters.TestName = $"{method.Name}.{method.Name} - {config}";
                parameters.Properties.Set("Config", config);
                yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, parameters);
            }
        }
    }
}

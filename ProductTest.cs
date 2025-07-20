using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebE2E
{
    [TestFixture]
    public class ProductTestFixture
    {
        public static IPage Page { get; private set; } = null!;
        public static IPlaywright Playwright { get; private set; } = null!;
        public static IBrowser Browser { get; private set; } = null!;
        public static IBrowserContext Context { get; private set; } = null!;

        public async Task RunTest(BrowserType browserType, string url, IMethodInfo testMethod)
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = browserType switch
            {
                BrowserType.Chromium => await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false }),
                BrowserType.Firefox => await Playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false }),
                BrowserType.Webkit => await Playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false }),
                _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
            };

            Context = await Browser.NewContextAsync(new() { BaseURL = url });

            Page = await Context.NewPageAsync();

            await Page.GotoAsync(url);

            var testInstance = Activator.CreateInstance(testMethod.TypeInfo.Type);
            var result = testMethod.MethodInfo.Invoke(testInstance, null);

            if(result is Task task)
            {
                await task;
            }
        }
    }

    public class ProductTest: NUnitAttribute, ITestBuilder, IImplyFixture
    {
        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite)
        {
            var testFixtureType = typeof(ProductTestFixture);
            var runTestMethod = new MethodWrapper(testFixtureType, nameof(ProductTestFixture.RunTest));

            foreach(BrowserType browser in Enum.GetValues(typeof(BrowserType)))
            {
                foreach(string url in TestSettings.ProductUrls)
                { 
                    var testParams = new object[] { browser, url, method };

                    var environment = url switch
                    {
                        "http://localhost:5173" => "Local",
                        "https://example.hosting.app" => "Stage",
                        "https://example.com" => "Prod",
                        _ => "Unknown"
                    };

                    var testCase = new TestCaseParameters(testParams)
                    {
                        TestName = $"{method.Name} - {browser} - {environment}",
                    };
                    yield return new NUnitTestCaseBuilder().BuildTestMethod(runTestMethod, suite, testCase);
                }
            }
        }
    }
}

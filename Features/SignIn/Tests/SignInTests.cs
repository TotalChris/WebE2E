using WebE2E.Features.SignIn.Pages;
using static Microsoft.Playwright.Assertions;

namespace WebE2E.Features.SignIn.Tests
{
    public class SignInTests : ProductTestFixture
    {
        [ProductTest]
        public async Task UserCanSignIn(TestConfig config)
        {
            var signInPage = new SignInPage(Page);
            await signInPage.SignInAsync("TestUser", "Password123!");
        }
    }
}

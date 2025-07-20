using Microsoft.Playwright;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebE2E.Features.SignIn.Pages
{
    class SignInPage
    {
        private readonly IPage page;
        private readonly ILocator usernameField;
        private readonly ILocator passwordField;
        private readonly ILocator signInButton;

        public SignInPage(IPage page)
        {
            this.page = page;
            this.usernameField = page.GetByLabel("Username", new() { Exact = true });
            this.passwordField = page.GetByLabel("Password", new() { Exact = true });
            this.signInButton = page.GetByText("Sign In");
        }

        public async Task SignInAsync(string username, string password)
        {
            await usernameField.FillAsync(username);
            await passwordField.FillAsync(password);
            await signInButton.ClickAsync();
        }
    }
}

using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace WebTechLab.Tests;

[TestFixture]
public class AuthTests : PageTest
{
    private readonly string _baseUrl = "https://localhost:7291";

    [Test]
    [Order(1)]
    public async Task ShouldRedirectUnauthorizedUserToLogin()
    {

        await Page.GotoAsync($"{_baseUrl}/events/create");

        await Expect(Page).ToHaveURLAsync(new Regex($"{_baseUrl}/Identity/Account/Login.*"));
    }

    [Test]
    [Order(2)]
    public async Task ShouldRegisterLoginAndLogoutSuccessfully()
    {
        var userEmail = $"testuser-{DateTime.Now.Ticks}@example.com";
        var userPassword = "Password123!";

        await Page.GotoAsync($"{_baseUrl}/Identity/Account/Register");

        await Page.GetByLabel("Email").FillAsync(userEmail);

        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(userPassword);
        await Page.GetByLabel("Confirm password").FillAsync(userPassword);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your account" }).ClickAsync();

        await Expect(Page.GetByText("Thank you for confirming your email.")).ToBeVisibleAsync();

        await Page.GotoAsync($"{_baseUrl}/Identity/Account/Login");

        await Page.GetByLabel("Email").FillAsync(userEmail);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(userPassword);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(Page.GetByText($"Hello {userEmail}!")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
    }
}
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace WebTechLab.Tests;

[TestFixture]
public class EventTests : PageTest
{
    private readonly string _baseUrl = "https://localhost:7291";

    [Test]
    public async Task ShouldCreateNewEventSuccessfully()
    {
        await Page.GotoAsync($"{_baseUrl}/Identity/Account/Login");

        await Page.GetByLabel("Email").FillAsync("tester@test.com");
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync("Password123!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex($"{_baseUrl}/$"));

        var eventTitle = $"Тестова Подія {DateTime.Now.Ticks}";

        await Page.GotoAsync($"{_baseUrl}/events/create");

        await Page.GetByLabel("Title").FillAsync(eventTitle);
        await Page.GetByLabel("EventPosterUrl").FillAsync("https://example.com/image.jpg");
        await Page.GetByLabel("StartTime").FillAsync("2025-12-01T12:00");

        await Page.Locator(".EasyMDEContainer .CodeMirror").ClickAsync();
        await Page.Keyboard.TypeAsync("Це опис з E2E тесту");

        await Page.Locator("#categorySelect + .select2-container").ClickAsync();
        await Page.GetByRole(AriaRole.Searchbox).FillAsync("Музика");
        await Page.Locator(".select2-results__option--highlighted").ClickAsync();

        await Page.Locator("#venueSelect + .select2-container").ClickAsync();
        await Page.GetByRole(AriaRole.Searchbox).FillAsync("Палац");
        await Page.Locator(".select2-results__option--highlighted").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create", Exact = true }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex($"{_baseUrl}/events$", RegexOptions.IgnoreCase));
        await Expect(Page.GetByText(eventTitle)).ToBeVisibleAsync();
    }
}

using Bogus;
using Customers.WebApp.Models;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages
{
    [Collection("Test collection")]
    public class UpdateCustomerTests
    {
        private readonly SharedTestContext _testContext;
        private readonly Faker<Customer> _customerGenerator = new Faker<Customer>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, SharedTestContext.ValidGitHubUsername)
            .RuleFor(x => x.DateOfBirth, faker => DateOnly.FromDateTime(faker.Person.DateOfBirth.Date));

        public UpdateCustomerTests(SharedTestContext testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Update_UpdateCustomer_WhenDataIsValid()
        {
            // Arrange
            var page = await _testContext.Browser.NewPageAsync(new BrowserNewPageOptions
            {
                BaseURL = SharedTestContext.AppUrl
            });

            var createdCustomer = await CreateCustomer(page);
            
            // Act
            await page.GotoAsync($"update-customer/{createdCustomer.Id}");
            var updatedCustomer = _customerGenerator.Generate();
            await page.FillAsync("input[id=fullname]", updatedCustomer.FullName);
            await page.FillAsync("input[id=email]", updatedCustomer.Email);
            await page.FillAsync("input[id=github-username]", updatedCustomer.GitHubUsername);
            await page.FillAsync("input[id=dob]", updatedCustomer.DateOfBirth.ToString("yyyy-MM-dd"));
            await page.ClickAsync("button[type=submit]");
            await page.GotoAsync($"customer/{createdCustomer.Id}");

            // Assert
            (await page.Locator("p[id=fullname-field]").InnerTextAsync()).Should().Be(updatedCustomer.FullName);
            (await page.Locator("p[id=email-field]").InnerTextAsync()).Should().Be(updatedCustomer.Email);
            (await page.Locator("p[id=github-username-field]").InnerTextAsync()).Should().Be(updatedCustomer.GitHubUsername);
            (await page.Locator("p[id=dob-field]").InnerTextAsync()).Should().Be(updatedCustomer.DateOfBirth.ToString("dd/MM/yyyy"));
        }

        [Fact]
        public async Task Update_ShowsError_WhenEmailIsInvalid()
        {

        }

        private async Task<Customer> CreateCustomer(IPage page)
        {
            await page.GotoAsync("add-customer");
            var customer = _customerGenerator.Generate();

            await page.FillAsync("input[id=fullname]", customer.FullName);
            await page.FillAsync("input[id=email]", customer.Email);
            await page.FillAsync("input[id=github-username]", customer.GitHubUsername);
            await page.FillAsync("input[id=dob]", customer.DateOfBirth.ToString("yyyy-MM-dd"));

            await page.ClickAsync("button[type=submit]");
            return customer;
        }
    }
}

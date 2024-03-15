using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController
{
    public class UpdateCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public UpdateCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task Update_UpdatesUser_WhenDataIsValid()
        {
            // Arrange
            var customer = _customerGenerator.Generate();
            var createResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomerResponse = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            var updateCustomerRequest = _customerGenerator.Generate();

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"customers/{createdCustomerResponse!.Id}", updateCustomerRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var updatedCustomer = await updateResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            updatedCustomer.Should().BeEquivalentTo(updateCustomerRequest);            
        }

        [Fact]
        public async Task Update_ReturnsValidationError_WhenEmailIsInvalid()
        {
            // Arrange
            const string invalidEmail = "dasdja9d3j";
            var customer = _customerGenerator.Generate();
            var createResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomerResponse = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            var updateCustomerRequest = _customerGenerator.Clone().RuleFor(x => x.Email, invalidEmail).Generate();

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"customers/{createdCustomerResponse!.Id}", updateCustomerRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var error = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Be("One or more validation errors occurred.");
            error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
        }

        [Fact]
        public async Task Update_ReturnsValidationError_WhenGitHubUserDoestNotExist()
        {
            // Arrange
            const string invalidGitHubUser = "dasdja9d3j";
            var customer = _customerGenerator.Generate();
            var createResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomerResponse = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            var updateCustomerRequest = _customerGenerator.Clone().RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"customers/{createdCustomerResponse!.Id}", updateCustomerRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var error = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Be("One or more validation errors occurred.");
            error.Errors["GitHubUsername"][0].Should().Be($"There is no GitHub user with username {invalidGitHubUser}");
        }
    }
}

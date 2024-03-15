using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController
{
    public class GetCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public GetCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task Get_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = _customerGenerator.Generate();
            var createdResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

            // Act
            var customerResponse = await _client.GetAsync($"customers/{createdCustomer!.Id}");

            // Assert
            var retreivedCustomer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();
            retreivedCustomer.Should().BeEquivalentTo(customer);
            customerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange


            // Act
            var customerResponse = await _client.GetAsync($"customers/{Guid.Empty}");

            // Assert
            customerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}

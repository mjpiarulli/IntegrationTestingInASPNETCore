using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using System.Net.Http.Json;
using Xunit;

namespace Customers.Api.Tests.Integration.CustomerController
{
    public class GetAllCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public GetAllCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ReturnsAllCustomers_WhenCustomersExist()
        {
            // Arrange
            var customer = _customerGenerator.Generate();
            var createdResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

            // Act
            var customersResponse = await _client.GetAsync($"customers");

            // Assert
            var retreivedCustomers = await customersResponse.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
            retreivedCustomers!.Customers.Single().Should().BeEquivalentTo(createdCustomer);
            customersResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyResult_WhenNoCustomersExist()
        {
            // Arrange
            var customersToCleanUpResponse = await _client.GetAsync($"customers");
            var customersToCleanUp = await customersToCleanUpResponse.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
            foreach (var customer in customersToCleanUp!.Customers)
                await _client.DeleteAsync($"customers/{customer.Id}");

            // Act
            var customersResponse = await _client.GetAsync($"customers");

            // Assert
            var retreivedCustomers = await customersResponse.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
            retreivedCustomers!.Customers.Should().BeEmpty();
            customersResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}

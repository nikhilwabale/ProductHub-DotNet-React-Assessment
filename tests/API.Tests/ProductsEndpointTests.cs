using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using Xunit;

namespace API.Tests;

public sealed class ProductsEndpointTests(AssessmentFactory factory) : IClassFixture<AssessmentFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ThenCreateAndFetchProduct_Succeeds()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("admin@crn.local", "Admin@123"));
        login.EnsureSuccessStatusCode();

        var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var create = await _client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest("Test Widget", "Integration test product", null));
        create.EnsureSuccessStatusCode();

        var created = await create.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.Equal("Test Widget", created!.ProductName);

        var get = await _client.GetAsync($"/api/v1/products/{created.Id}");
        get.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("admin@crn.local", "WrongPassword"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

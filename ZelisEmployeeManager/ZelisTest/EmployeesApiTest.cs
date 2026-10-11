using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZelisEmployeeManager.Data;

namespace ZelisTest.Integration;

public class EmployeesApiTests : IClassFixture<ApiFactory>
{
    private const string BaseUrl = "/api/employees";

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public EmployeesApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    //  helpers


    // build as json vs DTO class to exercise real contract
    // unique email per call because all tests in this class share one database

    private static JsonObject NewEmployee(string? email = null) => new()
    {
        ["firstName"] = "Test",
        ["lastName"] = "User",
        ["email"] = email ?? $"test-{Guid.NewGuid():N}@example.com",
        ["phone"] = "(555)-123-4567",
        ["address"] = new JsonObject
        {
            ["address1"] = "123 Main St",
            ["address2"] = null,
            ["city"] = "St. Louis",
            ["state"] = "MO",
            ["zip"] = "63123"
        }
    };

    private async Task<int> CreateAsync(JsonObject body)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    //  GET 

    [Fact]
    public async Task GetById_Missing_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //  POST 

    [Fact]
    public async Task Create_Valid_Returns201_WithLocationAndNestedAddress()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, NewEmployee());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("St. Louis", json.GetProperty("address").GetProperty("city").GetString());
    }

    [Theory]
    [InlineData("phone", "555-123-4567")]
    [InlineData("phone", "(555)123-4567")]
    [InlineData("phone", "(555)-1234-567")]
    [InlineData("email", "not-an-email")]
    [InlineData("firstName", "")]
    [InlineData("lastName", "")]
    [InlineData("address.PrimaryAddress", "")]
    [InlineData("address.city", "")]
    [InlineData("address.state", "Missouri")]
    [InlineData("address.state", "AAA")]
    [InlineData("address.zip", "1234")]
    [InlineData("address.zip", "123456")]
    [InlineData("address.zip", "abcde")]
    public async Task Create_InvalidField_Returns400(string field, string value)
    {
        var body = NewEmployee();
        var target = field.StartsWith("address.") ? body["address"]!.AsObject() : body;
        target[field.Replace("address.", "")] = value;

        var response = await _client.PostAsJsonAsync(BaseUrl, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAddress_Returns400()
    {
        var body = NewEmployee();
        body.Remove("address");

        var response = await _client.PostAsJsonAsync(BaseUrl, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateEmail_DifferentCase_Returns409()
    {
        var email = $"Case.{Guid.NewGuid():N}@Example.com";
        await CreateAsync(NewEmployee(email));

        var response = await _client.PostAsJsonAsync(BaseUrl, NewEmployee(email.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    //  PUT 

    [Fact]
    public async Task Update_Existing_Succeeds_AndPersists()
    {
        var body = NewEmployee();
        var id = await CreateAsync(body);

        body["id"] = id; // ignored if your DTO has no Id
        body["address"]!["city"] = "Kansas City";
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", body);

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Unexpected status {response.StatusCode}");

        var updated = await _client.GetFromJsonAsync<JsonElement>($"{BaseUrl}/{id}");
        Assert.Equal("Kansas City", updated.GetProperty("address").GetProperty("city").GetString());
    }

    [Fact]
    public async Task Update_Missing_Returns404()
    {
        var body = NewEmployee();
        body["id"] = 999999;

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/999999", body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToAnotherEmployeesEmail_Returns409()
    {
        var emailA = $"a-{Guid.NewGuid():N}@example.com";
        await CreateAsync(NewEmployee(emailA));
        var bodyB = NewEmployee();
        var idB = await CreateAsync(bodyB);

        bodyB["id"] = idB;
        bodyB["email"] = emailA.ToUpperInvariant();
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{idB}", bodyB);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    //  DELETE 

    [Fact]
    public async Task Delete_Existing_Succeeds_AndCascadesToAddress()
    {
        var id = await CreateAsync(NewEmployee());

        var response = await _client.DeleteAsync($"{BaseUrl}/{id}");

        Assert.True(response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK,
            $"Unexpected status {response.StatusCode}");
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"{BaseUrl}/{id}")).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Addresses.AnyAsync(a => a.EmployeeId == id));
    }

    [Fact]
    public async Task Delete_Missing_Returns404()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
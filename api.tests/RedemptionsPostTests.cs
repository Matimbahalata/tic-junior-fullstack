using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using api.tests;

namespace api.tests;

public class RedemptionsPostTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RedemptionsPostTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // TEST 1 — Happy path: valid request creates a redemption (201)
    [Fact]
    public async Task Create_WithValidRequest_Returns201()
    {
        var body = new { participantId = 2, rewardId = 13 };
        var response = await _client.PostAsJsonAsync("/api/redemptions", body);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", json.GetProperty("status").GetString());
        Assert.Equal(2, json.GetProperty("participantId").GetInt32());
        Assert.Equal(13, json.GetProperty("rewardId").GetInt32());
    }

    // TEST 2 — Rule 7: insufficient points (400)
    [Fact]
    public async Task Create_WithInsufficientPoints_Returns400()
    {
        var body = new { participantId = 4, rewardId = 11 }; // Lerato (100 pts), Voucher R100 (1000)
        var response = await _client.PostAsJsonAsync("/api/redemptions", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("not have enough points", json.GetProperty("message").GetString());
    }

    // TEST 3 — Rule 6: duplicate Pending for same reward (409)
    [Fact]
    public async Task Create_WithExistingPendingRedemption_Returns409()
    {
        var body = new { participantId = 2, rewardId = 11 }; // Timo already has Pending for 11
        var response = await _client.PostAsJsonAsync("/api/redemptions", body);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("pending redemption", json.GetProperty("message").GetString());
    }

    // TEST 4 — Rule 3: inactive participant (400)
    [Fact]
    public async Task Create_WithInactiveParticipant_Returns400()
    {
        var body = new { participantId = 3, rewardId = 10 }; // Sam is inactive
        var response = await _client.PostAsJsonAsync("/api/redemptions", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("not active", json.GetProperty("message").GetString());
    }

    // TEST 5 — Rule 5: inactive reward (400)
    [Fact]
    public async Task Create_WithInactiveReward_Returns400()
    {
        var body = new { participantId = 1, rewardId = 12 }; // Legacy Gift is inactive
        var response = await _client.PostAsJsonAsync("/api/redemptions", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("not currently available", json.GetProperty("message").GetString());
    }
}
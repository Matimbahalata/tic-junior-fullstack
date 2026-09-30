using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace api.tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core related registrations from the real API
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<RewardsDBContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                (d.ServiceType.FullName != null &&
                 d.ServiceType.FullName.Contains("EntityFrameworkCore"))
            ).ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Add the in-memory database for testing
            services.AddDbContext<RewardsDBContext>(options =>
{
                options.UseInMemoryDatabase("TestDb");
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RewardsDBContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(RewardsDBContext db)
    {
        if (db.Participants.Any()) return;

        db.Participants.AddRange(
            new Participant { ParticipantId = 1, FullName = "Amina Jacobs", Email = "amina@example.com", CountryCode = "ZA", PointsBalance = 2000, IsActive = true },
            new Participant { ParticipantId = 2, FullName = "Timo Negonga", Email = "timo@example.com", CountryCode = "NA", PointsBalance = 600, IsActive = true },
            new Participant { ParticipantId = 3, FullName = "Sam Greene", Email = "sam@example.com", CountryCode = "GB", PointsBalance = 1500, IsActive = false },
            new Participant { ParticipantId = 4, FullName = "Lerato Mokoena", Email = "lerato@example.com", CountryCode = "ZA", PointsBalance = 100, IsActive = true }
        );

        db.Rewards.AddRange(
            new Reward { RewardId = 10, Name = "Airtime R50", PointsCost = 500, IsActive = true },
            new Reward { RewardId = 11, Name = "Voucher R100", PointsCost = 1000, IsActive = true },
            new Reward { RewardId = 12, Name = "Legacy Gift", PointsCost = 200, IsActive = false },
            new Reward { RewardId = 13, Name = "Data Bundle 1GB", PointsCost = 300, IsActive = true }
        );

        db.Redemptions.AddRange(
            new Redemption
            {
                RedemptionId = 101,
                ParticipantId = 2,
                RewardId = 11,
                Status = "Pending",
                RequestedAt = new DateTime(2026, 9, 8, 14, 0, 0, DateTimeKind.Utc)
            }
        );

        db.SaveChanges();
    }
}
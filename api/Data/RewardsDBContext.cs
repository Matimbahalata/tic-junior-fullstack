using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data;

public partial class RewardsDBContext : DbContext
{
    public RewardsDBContext()
    {
    }

    public RewardsDBContext(DbContextOptions<RewardsDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Redemption> Redemptions { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId).HasName("PK__Particip__7227995E1E27F272");

            entity.Property(e => e.ParticipantId).ValueGeneratedNever();
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.HasKey(e => e.RedemptionId).HasName("PK__Redempti__410680B166DA9F85");

            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Participant).WithMany(p => p.Redemptions)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Redemptio__Parti__4D94879B");

            entity.HasOne(d => d.Reward).WithMany(p => p.Redemptions)
                .HasForeignKey(d => d.RewardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Redemptio__Rewar__4E88ABD4");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.RewardId).HasName("PK__Rewards__825015B96F114F0A");

            entity.Property(e => e.RewardId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

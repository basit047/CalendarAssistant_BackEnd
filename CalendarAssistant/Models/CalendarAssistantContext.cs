using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CalendarAssistant.Models;

public partial class CalendarAssistantContext : DbContext
{
    public CalendarAssistantContext()
    {
    }

    public CalendarAssistantContext(DbContextOptions<CalendarAssistantContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<DayWeek> DayWeeks { get; set; }

    public virtual DbSet<Meeting> Meetings { get; set; }

    public virtual DbSet<PollingSync> PollingSyncs { get; set; }

    public virtual DbSet<WorkingHour> WorkingHours { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-AF98UVH;Database=CalendarAssistant;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.UserId, "UQ__AspNetUs__1788CC4DA746E6D2").IsUnique();

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.TimeZoneId).HasMaxLength(70);
            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<DayWeek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DayWeek__3214EC074265920B");

            entity.ToTable("DayWeek");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Day).HasMaxLength(20);
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.MeetingId).HasName("PK__Meetings__E9F9E94CC6FBAD5C");

            entity.Property(e => e.Attendees).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.EndDateTime).HasColumnType("datetime");
            entity.Property(e => e.EventId).HasMaxLength(40);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.StartDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.MeetingCreatedByNavigations)
                .HasPrincipalKey(p => p.UserId)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Meetings__Create__6FE99F9F");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.MeetingModifiedByNavigations)
                .HasPrincipalKey(p => p.UserId)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK__Meetings__Modifi__70DDC3D8");
        });

        modelBuilder.Entity<PollingSync>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PollingSync");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.SyncDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany()
                .HasPrincipalKey(p => p.UserId)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PollingSy__UserI__151B244E");
        });

        modelBuilder.Entity<WorkingHour>(entity =>
        {
            entity.ToTable("WorkingHour");

            entity.Property(e => e.IsWorkingDay).HasDefaultValueSql("((1))");

            entity.HasOne(d => d.Day).WithMany(p => p.WorkingHours)
                .HasForeignKey(d => d.DayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkingHo__DayId__0C85DE4D");

            entity.HasOne(d => d.User).WithMany(p => p.WorkingHours)
                .HasPrincipalKey(p => p.UserId)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkingHo__UserI__0B91BA14");
        });

        modelBuilder.Entity<ScheduleModel>().HasNoKey();
        modelBuilder.Entity<UserTimeZoneMapping>().HasNoKey();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

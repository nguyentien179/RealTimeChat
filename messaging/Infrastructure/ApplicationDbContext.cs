using System;
using System.Reflection;
using messaging.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace messaging.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configuration from IEntityTypeConfiguration implementations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
}

using System;
using messaging.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace messaging.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ChatMessage> ChatMessages { get; set; }
}

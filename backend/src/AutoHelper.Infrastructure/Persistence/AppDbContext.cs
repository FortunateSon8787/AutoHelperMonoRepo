using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.AdCampaigns;
using AutoHelper.Domain.Admins;
using AutoHelper.Domain.Chatbot;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Common;
using AutoHelper.Domain.Customers;
using AutoHelper.Domain.Partners;
using AutoHelper.Domain.Reviews;
using AutoHelper.Domain.ServiceRecords;
using AutoHelper.Domain.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<ChatbotConfig> ChatbotConfigs => Set<ChatbotConfig>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminRefreshToken> AdminRefreshTokens => Set<AdminRefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SubscriptionPlanConfig> SubscriptionPlanConfigs => Set<SubscriptionPlanConfig>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AdCampaign> AdCampaigns => Set<AdCampaign>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<InvalidChatRequest> InvalidChatRequests => Set<InvalidChatRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves all changes and dispatches collected domain events after the transaction commits.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregatesWithEvents.ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}

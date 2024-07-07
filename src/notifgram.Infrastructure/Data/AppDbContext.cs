using System.Reflection;
using notifgram.Core;
using notifgram.Core.ChannelAggregate;
using notifgram.Core.PostAggregate;
using notifgram.SharedKernel;
using notifgram.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace notifgram.Infrastructure.Data;

public class AppDbContext : DbContext
{
  private readonly IDomainEventDispatcher? _dispatcher;

  public AppDbContext(DbContextOptions<AppDbContext> options,
    IDomainEventDispatcher? dispatcher)
      : base(options)
  {
    _dispatcher = dispatcher;
  }


  public DbSet<Channel> Channels => Set<Channel>();
  public DbSet<Post> Posts => Set<Post>();


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
    var entries = ChangeTracker
               .Entries()
               .Where(e => e.Entity is IAggregateRoot && (
                       e.State == EntityState.Added
                       || e.State == EntityState.Modified));

    foreach (var entityEntry in entries)
    {
      var dateTimeString = DateTime.UtcNow.ToString(Constants.LastUpdateDateTimeFormat);
      var dataTime = DateTime.ParseExact(dateTimeString, Constants.LastUpdateDateTimeFormat, null);

      if (entityEntry.Entity is Post postEntity)
      {
        postEntity.LastUpdate = dataTime;
      }
      else if (entityEntry.Entity is Channel channelEntity)
      {
        channelEntity.LastUpdate = dataTime;
      }
    }

    int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    // ignore events if no dispatcher provided
    if (_dispatcher == null) return result;

    // dispatch events only if save was successful
    var entitiesWithEvents = ChangeTracker.Entries<EntityBase>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .ToArray();

    await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);

    return result;
  }

  public override int SaveChanges()
  {
    var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAggregateRoot && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

    foreach (var entityEntry in entries)
    {
      var dateTimeString = DateTime.UtcNow.ToString(Constants.LastUpdateDateTimeFormat);
      var dataTime = DateTime.ParseExact(dateTimeString, Constants.LastUpdateDateTimeFormat, null);

      if (entityEntry.Entity is Post postEntity)
      {
        postEntity.LastUpdate = dataTime;
      }
      else if (entityEntry.Entity is Channel channelEntity)
      {
        channelEntity.LastUpdate = dataTime;
      }
    }
    //return SaveChangesAsync().GetAwaiter().GetResult();
    return base.SaveChanges();
  }

  //private applyChangeTracker
}

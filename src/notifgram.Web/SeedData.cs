using notifgram.Core;
using notifgram.Core.PostAggregate;
using notifgram.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace notifgram.Web;

public static class SeedData
{
  public static readonly Post Contributor1 = new Post{
    ChannelId= 0,
    Text= "post contains image",
    FileName= "a.jpg",
    MediaType=MediaTypes.IMAGE,
    LastUpdate= DateTime.ParseExact("20230101-001122.ff", Constants.LastUpdateDateTimeFormat, null)
  };


  public static void Initialize(IServiceProvider serviceProvider)
  {
    using (var dbContext = new AppDbContext(
        serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>(), null))
    {

      if (dbContext.Posts.Any())
      {
        return;   // DB has been seeded
      }

      PopulateTestData(dbContext);
    }
  }

  public static void PopulateTestData(AppDbContext dbContext)
  {
    foreach (var item in dbContext.Posts)
    {
      dbContext.Remove(item);
    }
    dbContext.SaveChanges();

    dbContext.Posts.Add(Contributor1);

    dbContext.SaveChanges();

  }
}

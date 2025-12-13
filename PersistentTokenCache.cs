using ExcelDataReader.Log;
using Logbook;
using Logbook.Data;
using Microsoft.Identity.Client;

//TODO: this class creates multiple token cashes for users, one for every login, change the logic to update existing cache if found
//TODO: encrypt cache
//TODO: check if it is necessary to store entire cache and not just the refresh token
/// <summary>
/// This class represents a persistent token cache
/// </summary>
public class PersistentTokenCache
{
    private readonly LogbookDBContext _context;
    Guid _userId {get; set;} = Guid.Empty;

    private Logbook.Models.TokenCache? entry;

    /// <summary>
    /// Creates a new persistent token cache
    /// </summary>
    /// <param name="context">The database context to use for storing the caches</param>
    public PersistentTokenCache(LogbookDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Enables the persistent token cache, ensures it will be updated accordingly
    /// </summary>
    /// <param name="tokenCache">The token cache to keep track of</param>
    public void Enable(ITokenCache tokenCache)
    {
        Logger.Log("enabling");
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    // public void SetUserId(string userId)
    // {
    //     Logger.Log($"setting userid to {userId}");
    //     _userId = userId;

    //     if (entry == null)
    //     {
    //         Logger.Log("entry is null");
    //         return;
    //     }

    //     entry.UserId = userId;
    //     _context.SaveChanges();
    // }

    /// <summary>
    /// Assigns the token cache to the user with the specified Id and saves
    /// </summary>
    /// <param name="userId">The user id to assign the cache to</param>
    public void SaveTokenCache(Guid userId)
    {
        if (entry == null)
        {
            Logger.Log("entry is null");
            return;
        }

        entry.UserId = userId;

        Logbook.Models.TokenCache? existingCache = _context.TokenCaches.Where(c => c.UserId == userId).FirstOrDefault();

        if (existingCache != null)
        {
            existingCache.CacheData = entry.CacheData;
            existingCache.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            _context.TokenCaches.Add(entry);
        }

        _context.SaveChanges();
    }
    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("before");
        Logger.Log($"looking for entry for user with id {_userId}", Logger.LogLevel.Warning);

        entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);

        if (entry != null) Logger.Log("entry found");

        if (entry?.CacheData != null)
        {
            Logger.Log("entry has cached data");
            args.TokenCache.DeserializeMsalV3(entry.CacheData);
        }
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("after");

        if (!args.HasStateChanged)
        {
            Logger.Log("state hasnt changed, exiting after");
            return;
        }

        Logger.Log($"after: userid is {_userId}");

        entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);

        Logger.Log($"No entry was found, creating new entry with userid {_userId} and adding to db");
        entry = new Logbook.Models.TokenCache
        {
            UserId = _userId,
            CacheData = args.TokenCache.SerializeMsalV3(),
            LastUpdated = DateTime.UtcNow
        };

        if (entry.UserId == Guid.Empty)
        {
            Logger.Log("after: saving changes");
        }

        Logger.Log("after has finished");
    }
}

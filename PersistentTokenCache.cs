using System.Text;
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
    Guid _userId { get; set; } = Guid.Empty;

    private Logbook.Models.TokenCache? entry;

    ITokenCache _tokenCache;
    /// <summary>
    /// Creates a new persistent token cache
    /// </summary>
    /// <param name="context">The database context to use for storing the caches</param>
    public PersistentTokenCache(LogbookDBContext context)
    {
        _context = context;
    }

    // /// <summary>
    // /// Enables the persistent token cache, ensures it will be updated accordingly
    // /// </summary>
    // /// <param name="tokenCache">The token cache to keep track of</param>
    // public void Enable(ITokenCache tokenCache)
    // {
    //     Logger.Log("enabling the tokencache");
    //     tokenCache.SetBeforeAccess(BeforeAccessNotification);
    //     tokenCache.SetAfterAccess(AfterAccessNotification);
    // }

    /// <summary>
    /// Enables the persistent token cache, ensures it will be updated accordingly
    /// </summary>
    /// <param name="tokenCache">The token cache to keep track of</param>
    /// <param name="userId">The user id the cache belongs to</param>
    public void Enable(ITokenCache tokenCache, Guid userId)
    {
        Logger.Log("enabling the tokencache");
        if (userId != default)
        {
            Logger.Log($"Setting the user id to {userId}");
            _userId = userId;
        }

        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }


    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("before access");
        Logger.Log(args.Account);
        byte[] cacheData = args.TokenCache.SerializeMsalV3();
        Logger.Log(Encoding.UTF8.GetString(cacheData));

        //if the arguments contain a user associate that user with this cache, else if a user is assosiated
        //we use that user, else we cancell this notification
        if (args.Account != null) _userId = Guid.Parse(args.Account.HomeAccountId.ObjectId);
        else if (_userId == Guid.Empty) return;

        // Guid id = Guid.Parse(args.Account.HomeAccountId.ObjectId);

        Logbook.Models.TokenCache? entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);

        if (entry?.CacheData == null)
        {
            Logger.Log("No cached data found");
            return;
        }

        Logger.Log("deserializing cached data");
        Logger.Log(Encoding.UTF8.GetString(entry.CacheData));

        args.TokenCache.DeserializeMsalV3(entry.CacheData, true);
        // if (args.Account == null)
        // {
        //     Logger.Log("no assosiated account found, ignoring this notification");
        //     return;
        // }

        // Logger.Log(args.Account.HomeAccountId.ObjectId);

        // Guid id = Guid.Parse(args.Account.HomeAccountId.ObjectId);

        // Logger.Log($"looking for entry for user with id {id}", Logger.LogLevel.Warning);
        // entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == id);

        // if (entry == null) Logger.Log("no such entry exist");

        // if (entry?.CacheData != null)
        // {
        //     Logger.Log("entry with cached data found");
        //     args.TokenCache.DeserializeMsalV3(entry.CacheData);
        // }
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("after");
        Logger.Log(args.Account);
        byte[] cacheData = args.TokenCache.SerializeMsalV3();
        Logger.Log(Encoding.UTF8.GetString(cacheData));

        if (!args.HasStateChanged)
        {
            Logger.Log("state hasnt changed, exiting after");
            return;
        }

        //if the arguments contain a user associate that user with this cache, else if a user is assosiated
        //we use that user, else we cancell this notification
        if (args.Account != null) _userId = Guid.Parse(args.Account.HomeAccountId.ObjectId);
        else if (_userId == Guid.Empty) return;

        // Guid id = Guid.Parse(args.Account.HomeAccountId.ObjectId);

        Logbook.Models.TokenCache? entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);

        if (entry?.CacheData == null)
        {
            entry = new Logbook.Models.TokenCache
            {
                UserId = _userId,
                CacheData = args.TokenCache.SerializeMsalV3(),
                LastUpdated = DateTime.UtcNow
            };

            _context.TokenCaches.Add(entry);
        }
        else
        {
            entry.CacheData = args.TokenCache.SerializeMsalV3();
            entry.LastUpdated = DateTime.UtcNow;
        }

        _context.SaveChanges();
        // args.TokenCache.DeserializeMsalV3(entry.CacheData);

        // // if (args.TokenCache == null)
        // // {
        // //     Logger.Log("No token cache assosiated with acount, ignoring this notification");
        // //     return;
        // // }

        // Guid id = Guid.Parse(args.Account.HomeAccountId.ObjectId);

        // entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == id);

        // if (entry == null) 
        // {
        //     Logger.Log("No entry found");
        //     return;
        // }

        // if (entry != null)
        // {
        // entry.CacheData = args.TokenCache.SerializeMsalV3();
        //     entry.LastUpdated = DateTime.UtcNow;
        // }
        // else
        // {
        //     Logger.Log($"No entry was found, creating new entry with userid {_userId} and adding to db");
        //     entry = new Logbook.Models.TokenCache
        //     {
        //         UserId = id,
        //         CacheData = args.TokenCache.SerializeMsalV3(),
        //         LastUpdated = DateTime.UtcNow
        //     };

        //     _context.TokenCaches.Add(entry);
        // }

        // _context.SaveChanges();
    }
}

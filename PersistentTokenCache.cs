using Logbook;
using Logbook.Data;
using Microsoft.Identity.Client;

//TODO: this class creates multiple token cashes for users, one for every login, change the logic to update existing cache if found
//TODO: encrypt cache
//TODO: check if it is necessary to store entire cache and not just the refresh token
public class PersistentTokenCache
{
    private readonly LogbookDBContext _context;
    string _userId;

    private Logbook.Models.TokenCache entry;

    public PersistentTokenCache(LogbookDBContext context)
    {
        _context = context;
    }

    public void Enable(ITokenCache tokenCache)
    {
        Logger.Log("enabling");
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    public void SetUserId(string userId)
    {
        if (entry == null) return;

        entry.UserId = userId;
        _context.SaveChanges();
    }

    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("before");

        entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);
        if (entry?.CacheData != null)
        {
            args.TokenCache.DeserializeMsalV3(entry.CacheData);
        }
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        Logger.Log("after");

        if (!args.HasStateChanged) return;


        entry = _context.TokenCaches.FirstOrDefault(t => t.UserId == _userId);
        if (entry == null)
        {
            entry = new Logbook.Models.TokenCache { UserId = _userId };
            _context.TokenCaches.Add(entry);
        }

        entry.CacheData = args.TokenCache.SerializeMsalV3();
        entry.LastUpdated = DateTime.UtcNow;
        // _context.SaveChanges();
    }
}

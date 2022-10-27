using Microsoft.Identity.Client;
using System.Threading;
using System.Web;

namespace EasyBL.WEBAPP.TokenStorage
{
    public class SessionTokenCache : TokenCache
    {
        private static ReaderWriterLockSlim sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string userId = string.Empty;
        string cacheId = string.Empty;
        HttpContext httpContext = null;

        public SessionTokenCache(string userId, HttpContext httpContext)
        {
            this.userId = userId;
            cacheId = userId + "_TokenCache";
            this.httpContext = httpContext;
            BeforeAccess = BeforeAccessNotification;
            AfterAccess = AfterAccessNotification;
            Load();
        }

        public override void Clear(string clientId)
        {
            base.Clear(clientId);
            httpContext.Session.Remove(cacheId);
        }

        private void Load()
        {
            sessionLock.EnterReadLock();
            Deserialize((byte[])httpContext.Session[cacheId]);
            sessionLock.ExitReadLock();
        }

        private void Persist()
        {
            sessionLock.EnterReadLock();

            // Optimistically set HasStateChanged to false. 
            // We need to do it early to avoid losing changes made by a concurrent thread.
            HasStateChanged = false;

            httpContext.Session[cacheId] = Serialize();
            sessionLock.ExitReadLock();
        }

        // Triggered right before ADAL needs to access the cache. 
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            // Reload the cache from the persistent store in case it changed since the last access. 
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                Persist();
            }
        }
    }
}

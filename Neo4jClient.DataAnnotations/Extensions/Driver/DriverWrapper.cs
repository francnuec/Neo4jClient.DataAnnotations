using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class DriverWrapper : BaseWrapper<IDriver>, IDriver
    {
        public DriverWrapper(IDriver driver) : base(driver) { }

        public IAsyncSession AsyncSession()
        {
            return GetSession(WrappedItem.AsyncSession());
        }

        public IAsyncSession AsyncSession(Action<SessionConfigBuilder> action)
        {
            return GetSession(WrappedItem.AsyncSession(action));
        }

        public Task CloseAsync()
        {
            return WrappedItem.CloseAsync();
        }

        public Task VerifyConnectivityAsync()
        {
            return WrappedItem.VerifyConnectivityAsync();
        }

        public Task<bool> SupportsMultiDbAsync()
        {
            return WrappedItem.SupportsMultiDbAsync();
        }

        public Config Config { get => WrappedItem.Config; }

        public void Dispose()
        {
            WrappedItem.Dispose();
        }
        //TODO: REMOVE COMMENTED CODE
        //public IAsyncSession AsyncSession()
        //{
        //    return GetSession(WrappedItem.Session());
        //}

        //public IAsyncSession AsyncSession(AccessMode defaultMode)
        //{
        //    return GetSession(WrappedItem.AsyncSession(defaultMode));
        //}

        //public IAsyncSession AsyncSession(string bookmark)
        //{
        //    return GetSession(WrappedItem.AsyncSession(bookmark));
        //}

        //public IAsyncSession Session(AccessMode defaultMode, string bookmark)
        //{
        //    return GetSession(WrappedItem.Session(defaultMode, bookmark));
        //}

        //public IAsyncSession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        //{
        //    return GetSession(WrappedItem.Session(defaultMode, bookmarks));
        //}

        //public IAsyncSession Session(IEnumerable<string> bookmarks)
        //{
        //    return GetSession(WrappedItem.Session(bookmarks));
        //}

        protected internal static IAsyncSession GetSession(IAsyncSession session)
        {
            if (session != null && !(session is SessionWrapper))
            {
                return new SessionWrapper(session);
            }

            return session;
        }
    }
}

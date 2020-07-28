using System;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class DriverWrapper : BaseWrapper<IDriver>, IDriver
    {
        public DriverWrapper(IDriver driver) : base(driver)
        {
        }

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

        public Config Config => WrappedItem.Config;

        public void Dispose()
        {
            WrappedItem.Dispose();
        }

        protected internal static IAsyncSession GetSession(IAsyncSession session)
        {
            if (session != null && !(session is SessionWrapper)) return new SessionWrapper(session);

            return session;
        }
    }
}
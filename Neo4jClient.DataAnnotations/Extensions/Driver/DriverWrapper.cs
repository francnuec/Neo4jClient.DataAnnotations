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

        public Task<IServerInfo> GetServerInfoAsync()
        {
            return WrappedItem.GetServerInfoAsync();
        }

        public Task<bool> TryVerifyConnectivityAsync()
        {
            return WrappedItem.TryVerifyConnectivityAsync();
        }

        public Task VerifyConnectivityAsync()
        {
            return WrappedItem.VerifyConnectivityAsync();
        }

        public Task<bool> SupportsMultiDbAsync()
        {
            return WrappedItem.SupportsMultiDbAsync();
        }

        public Task<bool> SupportsSessionAuthAsync()
        {
            return WrappedItem.SupportsSessionAuthAsync();
        }

        public IExecutableQuery<IRecord, IRecord> ExecutableQuery(string cypher)
        {
            return WrappedItem.ExecutableQuery(cypher);
        }

        public Task<bool> VerifyAuthenticationAsync(IAuthToken authToken)
        {
            return WrappedItem.VerifyAuthenticationAsync(authToken);
        }

        public Config Config => WrappedItem.Config;
        public bool Encrypted => WrappedItem.Encrypted;

        public void Dispose()
        {
            WrappedItem.Dispose();
        }

        protected internal static IAsyncSession GetSession(IAsyncSession session)
        {
            if (session != null && !(session is SessionWrapper)) return new SessionWrapper(session);

            return session;
        }

        public ValueTask DisposeAsync()
        {
            return WrappedItem.DisposeAsync();
        }
    }
}
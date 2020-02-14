using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class DriverWrapper : BaseWrapper<IDriver>, IDriver
    {
        public DriverWrapper(IDriver driver) : base(driver) { }

        public Uri Uri => WrappedItem.Uri;

        public void Close()
        {
            WrappedItem.Close();
        }

        public Task CloseAsync()
        {
            return WrappedItem.CloseAsync();
        }

        public void Dispose()
        {
            WrappedItem.Dispose();
        }

        public ISession Session()
        {
            return GetSession(WrappedItem.Session());
        }

        public ISession Session(AccessMode defaultMode)
        {
            return GetSession(WrappedItem.Session(defaultMode));
        }

        public ISession Session(string bookmark)
        {
            return GetSession(WrappedItem.Session(bookmark));
        }

        public ISession Session(AccessMode defaultMode, string bookmark)
        {
            return GetSession(WrappedItem.Session(defaultMode, bookmark));
        }

        public ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return GetSession(WrappedItem.Session(defaultMode, bookmarks));
        }

        public ISession Session(IEnumerable<string> bookmarks)
        {
            return GetSession(WrappedItem.Session(bookmarks));
        }

        protected internal static ISession GetSession(ISession session)
        {
            if (session != null && !(session is SessionWrapper))
            {
                return new SessionWrapper(session);
            }

            return session;
        }
    }
}

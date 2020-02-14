using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public abstract class BaseWrapper<T> : IEquatable<T>
    {
        public BaseWrapper(T item)
        {
            WrappedItem = item;
        }

        public virtual T WrappedItem { get; protected set; }

        public virtual bool Equals(T other)
        {
            if (other is BaseWrapper<T> otherWrapper)
            {
                return WrappedItem.Equals(otherWrapper.WrappedItem);
            }

            return WrappedItem.Equals(other);
        }
    }
}

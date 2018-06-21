using System;
using System.Collections.Generic;

namespace Alexandria
{
    public partial class Library
    {
        private class ResourceCache : Dictionary<string, IResource>, IDisposable
        {
            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        foreach (var resource in Values)
                            resource.Dispose();
                    }

                    disposedValue = true;
                }
            }
            
            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }

        private class ResourceCachePartition : Dictionary<Type, ResourceCache>, IDisposable
        {
            public ResourceCache EstablishCache<T>()
            {
                if (!TryGetValue(typeof(T), out var result))
                    this[typeof(T)] = result = new ResourceCache();

                return result;
            }

            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        foreach (var cache in Values)
                            cache.Dispose();
                    }

                    disposedValue = true;
                }
            }
            
            public void Dispose()
            {
                Dispose(true);
            }
            #endregion
        }
    }
}

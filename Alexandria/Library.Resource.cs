using System;
using System.IO;

namespace Alexandria
{
    public partial class Library
    {
        private interface IResource : IDisposable
        {
            object Item { get; }
        }

        private class Resource<T> : IResource
        {
            public Resource(string rootedPath, T item)
            {
                ResourceStreamPath = rootedPath;
                Item = item;
            }

            public string ResourceStreamPath { get; }
            public T Item { get; }

            object IResource.Item => Item;

            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        if (Item is IDisposable i)
                            i.Dispose();
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

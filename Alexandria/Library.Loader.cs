using System;
using System.Collections.Generic;
using System.IO;

namespace Alexandria
{
    public partial class Library
    {
        private interface ILoader : IDisposable
        {
            void AssignToLibrary(Library library);
            object LoadFromStream(Stream dataStream);
        }

        /// <summary>
        /// Base class for types that load resources from streams.
        /// </summary>
        /// <typeparam name="T">The resource type that this loader will create.</typeparam>
        public abstract class Loader<T> : ILoader
        {
            public Type ResourceType => typeof(T);

            /// <summary>
            /// Construct an instance of T given a stream from the library's filestores.
            /// DO NOT dispose the stream!
            /// </summary>
            /// <param name="dataStream"></param>
            /// <param name="library"></param>
            /// <returns></returns>
            public abstract T LoadFromStream(Stream dataStream, Library library);

            void ILoader.AssignToLibrary(Library library)
            {
                Library = library;
            }

            object ILoader.LoadFromStream(Stream dataStream)
            {
                return LoadFromStream(dataStream, Library);
            }

            private Library Library;

            #region IDisposable Support
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
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

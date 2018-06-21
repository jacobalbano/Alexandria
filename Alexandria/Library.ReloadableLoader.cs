using System;
using System.IO;

namespace Alexandria
{
    public partial class Library
    {
        private interface IUpdateCompatibleLoader : ILoader
        {
            void UpdateFromStream(object item, Stream dataStream);
        }
        
        /// <summary>
        /// Base class for loaders that can perform an in-place reload on resources they've created.
        /// </summary>
        public abstract class ReloadableLoader<T> : Loader<T>, IUpdateCompatibleLoader
        {
            /// <summary>
            /// Updates an object in-place with new data from a stream.
            /// DO NOT dispose the stream!
            /// </summary>
            /// <param name="item">The item to be reloaded.</param>
            /// <param name="dataStream">The stream to read data from.</param>
            public abstract void UpdateFromStream(T item, Stream dataStream);

            void IUpdateCompatibleLoader.UpdateFromStream(object item, Stream dataStream)
            {
                UpdateFromStream((T)item, dataStream);
            }
        }
    }
}

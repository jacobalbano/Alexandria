using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alexandria.Loaders
{
    class StringLoader : Library.Loader<string>
    {
        public override string LoadFromStream(Stream dataStream, Library library)
        {
            using (var stream = new StreamReader(dataStream))
                return stream.ReadToEnd();
        }
    }
}

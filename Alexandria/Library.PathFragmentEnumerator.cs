using System;

namespace Alexandria
{
    public sealed partial class Library : IDisposable
    {
        private class PathFragmentEnumerator
        {
            public string Current => fragments[index];

            public bool HasNext => index < fragments.Length - 1;

            public bool MoveNext() => ++index < fragments.Length;
            
            public PathFragmentEnumerator(string path)
            {
                fragments = path.Split(@"\/".ToCharArray());
            }

            private PathFragmentEnumerator() { }

            private int index = -1;
            private string[] fragments;
        }
    }
}

using System.Collections.Generic;

// Used for reverse sorting
class DescComparer<T> : IComparer<T> {
    public int Compare(T x, T y) {
        return Comparer<T>.Default.Compare(y, x);
    }
}

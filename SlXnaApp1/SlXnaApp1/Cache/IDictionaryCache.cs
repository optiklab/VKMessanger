using System.Collections.Generic;

namespace SlXnaApp1.Cache
{
    public interface IDictionaryCache<K, T>
        where K : struct
        where T : class
    {
        IList<T> GetItems(K id);

        void AddItem(K id, T item);

        void AddItems(K id, IList<T> items);

        void ReplaceItems(K id, IList<T> items);
    }
}

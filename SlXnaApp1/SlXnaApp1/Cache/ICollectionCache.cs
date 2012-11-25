using System.Collections.Generic;

namespace SlXnaApp1.Cache
{
    interface ICollectionCache<T>
        where T: class
    {
        IList<T> GetItems();

        void AddItem(T item);

        void AddItems(IList<T> items);

        void RenewItem(T item, T new_item);

        bool IsEmpty();
    }
}

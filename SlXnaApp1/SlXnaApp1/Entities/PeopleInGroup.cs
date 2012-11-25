using System.Collections.Generic;

namespace SlXnaApp1.Entities
{
    public class PeopleInGroup<T> : List<T>
        where T : class
    {
        public PeopleInGroup(string category)
        {
            Key = category;
        }

        public string Key { get; set; }

        public bool HasItems { get { return Count > 0; } }
    }
}

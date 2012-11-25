using System.Collections.Generic;

namespace SlXnaApp1.Entities
{
    public class ContactsByFirstName : List<PeopleInGroup<PhoneContact>>
    {
        private static readonly string Groups = "абвгдеёжзиклмнопрстуфхцчшщэюяabcdefghijklmnopqrstuvwxyz#";

        public ContactsByFirstName(List<PhoneContact> contacts)
        {
            var groups = new Dictionary<string, PeopleInGroup<PhoneContact>>();

            foreach (char c in Groups)
            {
                var group = new PeopleInGroup<PhoneContact>(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (PhoneContact contact in contacts)
            {
                groups[EntitiesHelpers.GetFirstNameKey(contact.ContactName)].Add(contact);
            }
        }
    }
}

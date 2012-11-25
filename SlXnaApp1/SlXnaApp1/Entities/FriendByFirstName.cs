using System.Collections.Generic;
using System.Linq;

namespace SlXnaApp1.Entities
{
    public class FriendByFirstName : List<PeopleInGroup<FriendViewModel>>
    {
        private static readonly string Groups = "абвгдеёжзиклмнопрстуфхцшщэюяabcdefghijklmnopqrstuvwxyz#";

        public FriendByFirstName(List<FriendViewModel> friends)
        {
            var groups = new Dictionary<string, PeopleInGroup<FriendViewModel>>();

            // Create hints group.
            var hintsGroup = new PeopleInGroup<FriendViewModel>(string.Empty);
            this.Add(hintsGroup);
            groups[string.Empty] = hintsGroup;

            // Create other groups.
            foreach (char c in Groups)
            {
                var group = new PeopleInGroup<FriendViewModel>(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            // Separate friends by hints and categorized.
            var hints = friends.Where(f => f.HintOrder >= 0 && f.HintOrder < 5).ToList();
            hints.Sort((h1, h2) =>
                {
                    if (h1.HintOrder == h2.HintOrder)
                        return 0;
                    else if (h1.HintOrder > h2.HintOrder)
                        return 1;
                    else
                        return -1;
                });
            var categorized = friends.Except(hints).ToList();

            // Fill hints group.
            groups[string.Empty].AddRange(hints);

            // Fill categorized groups.
            foreach (FriendViewModel friend in categorized)
            {
                groups[EntitiesHelpers.GetFirstNameKey(friend.FullName)].Add(friend);
            }
        }
    }
}

using SlXnaApp1.Json;
using System.Linq;

namespace SlXnaApp1.Entities
{
    public class EntitiesHelpers
    {
        /// <summary>
        /// Generates chat title depending on its type - dialog or group chat.
        /// </summary>
        public static string GetChatTitle(int id, bool IsConference)
        {
            string title = string.Empty;
            if (IsConference)
            {
                Dialog dialog = App.Current.EntityService.Dialogs.FirstOrDefault(x => x.ChatId == id);

                if (dialog != null)
                    title = dialog.Title;
            }
            else
            {
                // NOTE. If dialog is not conference, id is Uid.
                FriendViewModel friend = App.Current.EntityService.Friends.FirstOrDefault(x => x.Uid == id);

                if (friend != null)
                    title = friend.FullName;
                else
                {
                    // Sometimes messages may be sent by NON-FRIEND.
                    UserInfo info = App.Current.EntityService.OtherUsers.FirstOrDefault(x => x.Uid == id);

                    if (info != null)
                        title = info.FullName != null ? info.FullName : info.Uid.ToString();
                }
            }

            return title;
        }

        public static string GetFirstNameKey(string name)
        {
            char key = char.ToLower(name[0]);

            if ((key < 'a' || key > 'z') &&
                (key < 'а' || key > 'я'))
            {
                key = '#';
            }

            return key.ToString();
        }
    }
}

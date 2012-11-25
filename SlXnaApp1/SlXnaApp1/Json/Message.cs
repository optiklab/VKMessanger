using System.Collections.Generic;
using System.Runtime.Serialization;
using SlXnaApp1.Entities;

namespace SlXnaApp1.Json
{
    [DataContract]
    public class Message
    {
        #region Constructor

        public Message()
        {
            IsRead = false;
            IsOut = false;
            Mid = -1;
            Uid = -1;
            AdminId = -1;
            Chatid = -1;
            UsersCount = 0;
            Title = string.Empty;
            Body = string.Empty;
            ChatActive = string.Empty;
        }

        #endregion

        #region Public properties

        [DataMember(Name="mid")]
        public int Mid { get; set; }

        [DataMember(Name = "chat_id")]
        public int Chatid { get; set; }

        [DataMember(Name = "chat_active")]
        public string ChatActive { get; set; }

        [DataMember(Name = "uid")]
        public int Uid { get; set; }

        [DataMember(Name = "date")]
        public int Date { get; set; }

        [DataMember(Name = "read_state")]
        public bool IsRead { get; set; }

        [DataMember(Name = "out")]
        public bool IsOut { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "users_count")]
        public int UsersCount { get; set; }

        [DataMember(Name = "admin_id")]
        public int AdminId { get; set; }

        [DataMember(Name = "deleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "geo")]
        public GeoAttachment GeoLocation { get; set; }

        [DataMember(Name = "attachments")]
        public IList<Attachment> Attachments { get; set; }

        [DataMember(Name = "fwd_messages")]
        public IList<Message> FwdMessages { get; set; }

        public int Flags { get; set; }

        #endregion

        #region Public methods

        public void ConsiderRemovingFlags(int mask)
        {
            // Now we REVERT all the values.
            if ((mask & 512) != 0) // MEDIA сообщение содержит медиаконтент 
            { }
            if ((mask & 256) != 0) // FIXED сообщение проверено пользователем на спам
            { }
            if ((mask & 128) != 0) // DELЕTЕD сообщение удалено (в корзине)
                IsDeleted = false;
            if ((mask & 64) != 0) // SPAM сообщение помечено как "Спам"
            { }
            if ((mask & 32) != 0) // FRIENDS сообщение отправлено другом
            { }
            if ((mask & 16) != 0) // CHAT сообщение отправлено через чат
            { }
            if ((mask & 8) != 0) // IMPORTANT помеченное сообщение
            { }
            if ((mask & 4) != 0) //REPLIED на сообщение был создан ответ
            { }
            if ((mask & 2) != 0) //OUTBOX исходящее сообщение
                IsOut = false;
            if ((mask & 1) != 0) //UNREAD сообщение не прочитано
                IsRead = true;
        }

        #endregion
    }
}

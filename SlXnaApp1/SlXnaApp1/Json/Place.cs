using System.Runtime.Serialization;

namespace SlXnaApp1.Json
{
    [DataContract(Name="place")]
    public class Place
    {
        [DataMember(Name = "place_id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }
    }
}

namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class ChatUnreadDto
    {
        public Guid ChatId { get; set; } // Veranderde van int naar Guid
        public bool HasUnread { get; set; }
    }
}
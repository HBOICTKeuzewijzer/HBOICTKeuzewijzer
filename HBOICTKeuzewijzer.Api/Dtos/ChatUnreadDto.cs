namespace HBOICTKeuzewijzer.Api.Dtos
{
    public class ChatUnreadDto
    {
        public Guid ChatId { get; set; }
        public bool HasUnread { get; set; }
    }
}
namespace RMQS.Application.Queue
{
    public class MessageC
    {
        public string CreatedAt { get; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public string Uid { get; } = Guid.NewGuid().ToString();
        public string Comment { get; set; } = string.Empty;
    }
}

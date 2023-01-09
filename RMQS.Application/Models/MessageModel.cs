public class MessageModel {
    public string MessageType {get;set;} = string.Empty;
    public string Comment {get;set;} = string.Empty;
    public bool WithException { get; set; } = false;
}
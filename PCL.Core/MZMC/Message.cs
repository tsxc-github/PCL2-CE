namespace PCL.Core.MZMC
{
    public class Message
    {
        public bool OK;
        public string MessageText;
        public dynamic Data;

        public Message(bool ok, string messageText, dynamic data)
        {
            OK = ok;
            MessageText = messageText;
            Data = data;
        }
        public Message(bool ok, string messageText)
        {
            OK = ok;
            MessageText = messageText;
        }
    }
}
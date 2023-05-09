using Marketplace.Modules.Marketplace_NPC;

namespace Marketplace.Modules.PostMail;

public static class PostMail_DataTypes
{
    public class MailData
    {
        public int UID;
        public string Sender;
        public string Message;
        public bool HasAttachedItem;
        public long TotalSeconds;
        public Marketplace_DataTypes.ClientMarketSendData AttachedItem;
    }
    
    
}
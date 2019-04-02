
namespace TileFortress.Net
{
    public enum DataMessageType : byte
    {
        Unknown,
        ChatMessage,
        ChunkRequest,
        ChunkData,
        BuildOrders
    }
}

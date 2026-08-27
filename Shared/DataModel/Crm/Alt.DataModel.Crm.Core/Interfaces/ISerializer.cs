
namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface ISerializer
    {
        string SerializeObject<T>(T content);
        T DeserializeObject<T>(string content);
    }
}

using MongoDB.Bson.Serialization.Attributes;

namespace CRUD
{
    public interface IDocument
    {
        [BsonElement("key")]
        string Key { get; set; }
    }
}

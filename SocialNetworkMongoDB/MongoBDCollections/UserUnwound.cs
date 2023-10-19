using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SocialNetworkMongoDB.MongoBDCollections;

[BsonIgnoreExtraElements]
internal class UserUnwound
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("posts")]
    public Post? Posts { get; set; }
}
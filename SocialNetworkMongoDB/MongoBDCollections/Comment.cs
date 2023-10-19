using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SocialNetworkMongoDB.MongoBDCollections;


[BsonIgnoreExtraElements]
internal class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("user_id")]
    public string? UserId { get; set; }

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    [BsonElement("date")]
    public DateTime Date { get; set; }

    public override string ToString() => Content!.Length >= 15 ? $"{Content[..15]}..." : Content;
}

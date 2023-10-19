using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetworkMongoDB.MongoBDCollections;

[BsonIgnoreExtraElements]
internal class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("categories")]
    public List<string>? Categories { get; set; }

    [BsonElement("likes")]
    public List<string>? Likes { get; set; }

    [BsonElement("dislikes")]
    public List<string>? Dislikes { get; set; }

    [BsonElement("comments")]
    public List<Comment>? Comments { get; set; }

    public override string ToString() => Content!.Length >= 15 ? $"{Content[..15]}..." : Content;
}

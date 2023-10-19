using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialNetworkMongoDB.MongoBDCollections;

[BsonIgnoreExtraElements]
internal class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("first_name")]
    public string? FirstName { get; set; }

    [BsonElement("last_name")]
    public string? LastName { get; set; }

    [BsonElement("interests")]
    public List<string>? Interests { get; set; }

    [BsonElement("friends")]
    public List<string>? Friends { get; set; }

    [BsonElement("posts")]
    public List<Post>? Posts { get; set; }

    public override string ToString() => $"{FirstName} {LastName}";
}
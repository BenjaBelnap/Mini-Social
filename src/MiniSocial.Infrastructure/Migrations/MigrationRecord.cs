using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiniSocial.Infrastructure.Migrations;

/// <summary>
/// Represents a migration record stored in the database.
/// </summary>
public class MigrationRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The version identifier of the migration (e.g., "001", "002").
    /// </summary>
    [BsonElement("version")]
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description of the migration.
    /// </summary>
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// When the migration was applied.
    /// </summary>
    [BsonElement("appliedAt")]
    public DateTime AppliedAt { get; set; }
    
    /// <summary>
    /// The name of the migration class.
    /// </summary>
    [BsonElement("migrationClass")]
    public string MigrationClass { get; set; } = string.Empty;
}

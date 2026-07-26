namespace CarOrganizer.Infrastructure.Storage;

/// <summary>Strongly-typed file storage configuration, bound from the "Storage" configuration section.</summary>
public class FileStorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Directory <see cref="LocalFileStorage"/> writes uploads to. A relative path is resolved against
    /// the application's content root. Replaced by bucket settings when R2 arrives in Phase 8.
    /// </summary>
    public string LocalRoot { get; set; } = "App_Data/documents";
}

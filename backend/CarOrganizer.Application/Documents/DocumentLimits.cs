namespace CarOrganizer.Application.Documents;

/// <summary>
/// Bounds and accepted formats for uploads, kept in one place so the controller's checks, the
/// request size limit and the tests can't drift apart.
/// </summary>
public static class DocumentLimits
{
    /// <summary>
    /// Phone photos run 3–8 MB and scanned multi-page PDFs larger, so the cap clears both while
    /// staying under Kestrel's 30 MB default request body limit (no extra configuration needed).
    /// </summary>
    public const int MaxFileSizeBytes = 15 * 1024 * 1024;

    /// <summary>Matches the <c>FileName</c> column length in <c>DocumentConfiguration</c>.</summary>
    public const int FileNameMaxLength = 255;

    /// <remarks>
    /// HEIC is deliberately absent. iPhones shoot it by default, but browsers can't render it, so
    /// accepting it would store bytes the later web client couldn't display; the mobile app converts
    /// to JPEG on the way out instead (Expo's ImagePicker does this by default).
    /// </remarks>
    private static readonly string[] Allowed =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
    ];

    /// <summary>What a vehicle's paperwork may be: photos of invoices and stickers, or PDFs.</summary>
    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(Allowed, StringComparer.OrdinalIgnoreCase);

    /// <summary>The accepted types as they should appear in a validation message.</summary>
    public static readonly string AllowedContentTypesDisplay = string.Join(", ", Allowed);
}

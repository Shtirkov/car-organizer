namespace CarOrganizer.Application.Documents;

/// <summary>
/// A document's bytes plus what the caller needs to render or save them. Ownership of
/// <see cref="Content"/> passes to the caller, which is responsible for disposing it.
/// </summary>
public record DocumentDownload(Stream Content, string ContentType, string FileName);

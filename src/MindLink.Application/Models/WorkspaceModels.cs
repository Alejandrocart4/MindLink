namespace MindLink.Application.Models;

public sealed record WorkspaceSnapshot(
    WorkspaceUser User,
    string ActiveProjectId,
    IReadOnlyList<WorkspaceMetric> Metrics,
    IReadOnlyList<WorkspaceQuickAction> QuickActions,
    IReadOnlyList<WorkspaceTask> PendingTasks,
    IReadOnlyList<WorkspaceProject> Projects,
    IReadOnlyList<WorkspaceNote> Notes,
    IReadOnlyList<WorkspaceActivity> Activities,
    IReadOnlyList<WorkspaceReference> References,
    IReadOnlyList<WorkspaceCitation> Citations,
    KnowledgeGraphSnapshot KnowledgeGraph,
    WorkspaceDocument ActiveDocument,
    IReadOnlyList<DocumentVersion> Versions,
    IReadOnlyList<ExportOption> ExportOptions,
    ExportSummary ExportSummary);

public sealed record WorkspaceUser(
    string Id,
    string FullName,
    string FirstName,
    string Email,
    string Role,
    string Initials);

public sealed record WorkspaceMetric(
    string Id,
    string Label,
    int Value,
    string Detail,
    string IconKey,
    string AccentKey);

public sealed record WorkspaceQuickAction(
    string Id,
    string Label,
    string Description,
    string Route,
    string IconKey);

public sealed record WorkspaceTask(
    string Id,
    string Title,
    string Detail,
    string Priority,
    string DueLabel,
    bool IsCompleted);

public sealed record WorkspaceProject(
    string Id,
    string Title,
    string ShortTitle,
    string Description,
    string Status,
    int Progress,
    int NotesCount,
    int ReferencesCount,
    string UpdatedLabel,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> CollaboratorInitials);

public sealed record WorkspaceNote(
    string Id,
    string ProjectId,
    string Title,
    string Excerpt,
    string Status,
    int ReferencesCount,
    int ConnectionsCount,
    string UpdatedLabel,
    IReadOnlyList<string> Tags);

public sealed record WorkspaceActivity(
    string Id,
    string Type,
    string Title,
    string Detail,
    string TimeLabel,
    string EntityId,
    string IconKey);

public sealed record WorkspaceReference(
    string Id,
    string ProjectId,
    string Authors,
    int Year,
    string Title,
    string Source,
    string ReferenceType,
    string ReadingStatus,
    int UsageCount,
    bool IsComplete,
    bool IsClassified,
    string Identifier,
    IReadOnlyList<string> Tags);

public sealed record WorkspaceCitation(
    string Id,
    string ReferenceId,
    string NoteId,
    string Quote,
    string SourceLabel,
    string PageLabel,
    string Context,
    IReadOnlyList<string> Tags);

public sealed record KnowledgeGraphSnapshot(
    string SelectedNodeId,
    IReadOnlyList<KnowledgeNode> Nodes,
    IReadOnlyList<KnowledgeConnection> Connections,
    IReadOnlyList<KnowledgeLegendItem> Legend);

public sealed record KnowledgeNode(
    string Id,
    string Label,
    string Subtitle,
    string Kind,
    double X,
    double Y,
    double Size,
    string AccentKey);

public sealed record KnowledgeConnection(
    string Id,
    string SourceNodeId,
    string TargetNodeId,
    string Label);

public sealed record KnowledgeLegendItem(
    string Kind,
    string Label,
    int Count,
    string AccentKey);

public sealed record WorkspaceDocument(
    string Id,
    string ProjectId,
    string Title,
    string ContextLabel,
    string SelectedSectionId,
    int TotalWordCount,
    string LastSavedLabel,
    IReadOnlyList<DocumentSection> Sections,
    IReadOnlyList<RelatedDocumentReference> RelatedReferences,
    IReadOnlyList<string> ConnectedNoteIds);

public sealed record DocumentSection(
    string Id,
    string? ParentId,
    string Number,
    string Title,
    string Content,
    string Status,
    int WordCount,
    int EvidenceCount,
    int Order);

public sealed record RelatedDocumentReference(
    string ReferenceId,
    int RelevancePercent);

public sealed record DocumentVersion(
    string Id,
    string Label,
    bool IsCurrent,
    string TimeLabel,
    string SectionLabel,
    int AddedWords,
    int RemovedWords,
    string Summary,
    IReadOnlyList<VersionChange> Changes);

public sealed record VersionChange(
    string Id,
    string ChangeType,
    string PreviousText,
    string CurrentText);

public sealed record ExportOption(
    string Id,
    string Title,
    string FileExtension,
    string Description,
    string IconKey,
    bool IsSelected,
    bool IsRecommended);

public sealed record ExportSummary(
    string ProjectTitle,
    string Format,
    string CitationStyle,
    int IncludedSections,
    int TotalSections,
    int IncludedReferences,
    int EstimatedPages);

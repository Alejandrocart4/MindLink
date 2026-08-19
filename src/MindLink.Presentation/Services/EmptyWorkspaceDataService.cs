using MindLink.Application.Interfaces;
using MindLink.Application.Models;
using MindLink.Presentation.ViewModels;
using System.Text.Json;
using System.IO;

namespace MindLink.Presentation.Services;

public sealed class EmptyWorkspaceDataService : IWorkspaceDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly SemaphoreSlim SaveLock = new(1, 1);

    public async Task<WorkspaceSnapshot> GetWorkspaceAsync(int userId, CancellationToken cancellationToken = default)
    {
        var path = GetWorkspacePath(userId);
        if (File.Exists(path))
        {
            WorkspaceSnapshot? saved;
            await using (var stream = File.OpenRead(path))
            {
                saved = await JsonSerializer.DeserializeAsync<WorkspaceSnapshot>(stream, JsonOptions, cancellationToken);
            }

            if (saved is not null)
            {
                var normalized = NormalizeSpanish(saved);
                await SaveWorkspaceAsync(userId, normalized, cancellationToken);
                return normalized;
            }
        }

        return AppSession.Plan == "Pro" ? CreateProWorkspace(userId) : CreateFreeWorkspace(userId);
    }

    public async Task SaveWorkspaceAsync(int userId, WorkspaceSnapshot workspace, CancellationToken cancellationToken = default)
    {
        var path = GetWorkspacePath(userId);
        var temporaryPath = $"{path}.tmp";
        await SaveLock.WaitAsync(cancellationToken);
        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, workspace, JsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, path, true);
        }
        finally
        {
            SaveLock.Release();
        }
    }

    private static string GetWorkspacePath(int userId)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MindLink", "workspaces");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"workspace-{userId}.json");
    }

    private static WorkspaceSnapshot NormalizeSpanish(WorkspaceSnapshot workspace) => workspace with
    {
        Metrics = workspace.Metrics.Select(item => item with { Label = CorrectText(item.Label), Detail = CorrectText(item.Detail) }).ToArray(),
        QuickActions = workspace.QuickActions.Select(item => item with { Label = CorrectText(item.Label), Description = CorrectText(item.Description) }).ToArray(),
        PendingTasks = workspace.PendingTasks.Select(item => item with { Title = CorrectText(item.Title), Detail = CorrectText(item.Detail), Priority = CorrectText(item.Priority), DueLabel = CorrectText(item.DueLabel) }).ToArray(),
        Projects = workspace.Projects.Select(item => item with { Title = CorrectText(item.Title), ShortTitle = CorrectText(item.ShortTitle), Description = CorrectText(item.Description), Status = CorrectText(item.Status), Tags = item.Tags.Select(CorrectText).ToArray() }).ToArray(),
        Notes = workspace.Notes.Select(item => item with { Title = CorrectText(item.Title), Excerpt = CorrectText(item.Excerpt), Status = CorrectText(item.Status), Tags = item.Tags.Select(CorrectText).ToArray() }).ToArray(),
        Activities = workspace.Activities.Select(item => item with { Type = CorrectText(item.Type), Title = CorrectText(item.Title), Detail = CorrectText(item.Detail), TimeLabel = CorrectText(item.TimeLabel) }).ToArray(),
        References = workspace.References.Select(item => item with { Authors = CorrectText(item.Authors), Title = CorrectText(item.Title), Source = CorrectText(item.Source), ReferenceType = CorrectText(item.ReferenceType), ReadingStatus = CorrectText(item.ReadingStatus), Tags = item.Tags.Select(CorrectText).ToArray() }).ToArray(),
        Citations = workspace.Citations.Select(item => item with { Quote = CorrectText(item.Quote), SourceLabel = CorrectText(item.SourceLabel), PageLabel = CorrectText(item.PageLabel), Context = CorrectText(item.Context), Tags = item.Tags.Select(CorrectText).ToArray() }).ToArray(),
        KnowledgeGraph = workspace.KnowledgeGraph with
        {
            Nodes = workspace.KnowledgeGraph.Nodes.Select(item => item with { Label = CorrectText(item.Label), Subtitle = CorrectText(item.Subtitle), Kind = CorrectText(item.Kind) }).ToArray(),
            Connections = workspace.KnowledgeGraph.Connections.Select(item => item with { Label = CorrectText(item.Label) }).ToArray(),
            Legend = workspace.KnowledgeGraph.Legend.Select(item => item with { Kind = CorrectText(item.Kind), Label = CorrectText(item.Label) }).ToArray()
        },
        ActiveDocument = workspace.ActiveDocument with
        {
            Title = CorrectText(workspace.ActiveDocument.Title),
            ContextLabel = CorrectText(workspace.ActiveDocument.ContextLabel),
            LastSavedLabel = CorrectText(workspace.ActiveDocument.LastSavedLabel),
            Sections = workspace.ActiveDocument.Sections.Select(item => item with { Title = CorrectText(item.Title), Content = CorrectText(item.Content), Status = CorrectText(item.Status) }).ToArray()
        },
        Versions = workspace.Versions.Select(item => item with
        {
            Label = CorrectText(item.Label),
            TimeLabel = CorrectText(item.TimeLabel),
            SectionLabel = CorrectText(item.SectionLabel),
            Summary = CorrectText(item.Summary),
            Changes = item.Changes.Select(change => change with { ChangeType = CorrectText(change.ChangeType), PreviousText = CorrectText(change.PreviousText), CurrentText = CorrectText(change.CurrentText) }).ToArray()
        }).ToArray(),
        ExportOptions = workspace.ExportOptions.Select(item => item with { Title = CorrectText(item.Title), Description = CorrectText(item.Description) }).ToArray(),
        ExportSummary = workspace.ExportSummary with { ProjectTitle = CorrectText(workspace.ExportSummary.ProjectTitle), CitationStyle = CorrectText(workspace.ExportSummary.CitationStyle) }
    };

    private static string CorrectText(string value) => value
        .Replace("Analisis", "Análisis", StringComparison.Ordinal)
        .Replace("analisis", "análisis", StringComparison.Ordinal)
        .Replace("Adopcion", "Adopción", StringComparison.Ordinal)
        .Replace("adopcion", "adopción", StringComparison.Ordinal)
        .Replace("Energia", "Energía", StringComparison.Ordinal)
        .Replace("energia", "energía", StringComparison.Ordinal)
        .Replace("Investigacion", "Investigación", StringComparison.Ordinal)
        .Replace("investigacion", "investigación", StringComparison.Ordinal)
        .Replace("obstaculos", "obstáculos", StringComparison.Ordinal)
        .Replace("apropiacion", "apropiación", StringComparison.Ordinal)
        .Replace("Diseno", "Diseño", StringComparison.Ordinal)
        .Replace("diseno", "diseño", StringComparison.Ordinal)
        .Replace("metodologico", "metodológico", StringComparison.Ordinal)
        .Replace("Metodologia", "Metodología", StringComparison.Ordinal)
        .Replace("metodologia", "metodología", StringComparison.Ordinal)
        .Replace("Hace 2 dias", "Hace 2 días", StringComparison.Ordinal)
        .Replace("Leido", "Leído", StringComparison.Ordinal)
        .Replace("Transicion", "Transición", StringComparison.Ordinal)
        .Replace("transicion", "transición", StringComparison.Ordinal)
        .Replace("energetica", "energética", StringComparison.Ordinal)
        .Replace("Politica", "Política", StringComparison.Ordinal)
        .Replace("politica", "política", StringComparison.Ordinal)
        .Replace("Gonzalez", "González", StringComparison.Ordinal)
        .Replace("Articulo", "Artículo", StringComparison.Ordinal)
        .Replace("articulo", "artículo", StringComparison.Ordinal)
        .Replace("cientifico", "científico", StringComparison.Ordinal)
        .Replace("Introduccion", "Introducción", StringComparison.Ordinal)
        .Replace("introduccion", "introducción", StringComparison.Ordinal)
        .Replace("energeticos", "energéticos", StringComparison.Ordinal)
        .Replace("acompan", "acompañ", StringComparison.Ordinal)
        .Replace("Participacion", "Participación", StringComparison.Ordinal)
        .Replace("participacion", "participación", StringComparison.Ordinal)
        .Replace("Marco teorico", "Marco teórico", StringComparison.Ordinal)
        .Replace("marco teorico", "marco teórico", StringComparison.Ordinal)
        .Replace("energias", "energías", StringComparison.Ordinal)
        .Replace("credito", "crédito", StringComparison.Ordinal)
        .Replace("redaccion", "redacción", StringComparison.Ordinal)
        .Replace("Version", "Versión", StringComparison.Ordinal)
        .Replace("Adicion", "Adición", StringComparison.Ordinal)
        .Replace("agrego", "agregó", StringComparison.Ordinal)
        .Replace("portatil", "portátil", StringComparison.Ordinal)
        .Replace("sin titulo", "sin título", StringComparison.Ordinal)
        .Replace("Nueva seccion", "Nueva sección", StringComparison.Ordinal)
        .Replace("En progreso", "En proceso", StringComparison.Ordinal);

    private static WorkspaceSnapshot CreateFreeWorkspace(int userId)
    {
        var user = CreateUser(userId, "Plan Free");
        return CreateSnapshot(user, string.Empty, [], [], [], [], [], [], [], [], EmptyDocument(), [], new KnowledgeGraphSnapshot(string.Empty, [], [], []));
    }

    private static WorkspaceSnapshot CreateProWorkspace(int userId)
    {
        var user = CreateUser(userId, "Plan Pro");
        const string projectId = "project-renewable-energy";
        var project = new WorkspaceProject(projectId, "Energía renovable comunitaria", "Energía renovable", "Análisis de la adopción de energía solar en comunidades rurales.", "Investigación", 58, 3, 3, "Hace 12 min", ["Energía", "Desarrollo local"], ["CP", "AR"]);
        var notes = new[]
        {
            new WorkspaceNote("note-barriers", projectId, "Barreras de adopción", "El costo inicial y la falta de financiamiento aparecen como los principales obstáculos.", "Revisada", 2, 3, "Hace 12 min", ["Financiamiento", "Barreras"]),
            new WorkspaceNote("note-impact", projectId, "Impacto social", "Las cooperativas locales fortalecen el mantenimiento y la apropiación del proyecto.", "Borrador", 1, 2, "Ayer", ["Comunidad", "Impacto"]),
            new WorkspaceNote("note-method", projectId, "Diseño metodológico", "Combinar entrevistas semiestructuradas con una encuesta de hogares.", "Borrador", 0, 1, "Hace 2 días", ["Metodología"])
        };
        var references = new[]
        {
            new WorkspaceReference("ref-iea", projectId, "International Energy Agency", 2024, "Renewables 2024", "IEA", "Informe institucional", "Leído", 3, true, true, "https://www.iea.org/reports/renewables-2024", ["Energía", "Mercado"]),
            new WorkspaceReference("ref-irena", projectId, "IRENA", 2023, "World Energy Transitions Outlook", "International Renewable Energy Agency", "Informe institucional", "En lectura", 2, true, true, "https://www.irena.org/Publications", ["Transición energética", "Política"]),
            new WorkspaceReference("ref-finance", projectId, "González, M.", 2022, "Financiamiento de energía solar rural", "Revista de Desarrollo Territorial", "Artículo científico", "Por leer", 1, true, true, "doi:10.0000/solar-rural", ["Financiamiento", "Rural"])
        };
        var document = new WorkspaceDocument("doc-energy", projectId, "Adopción de energía solar comunitaria", "Energía renovable comunitaria", "section-problem", 486, "Guardado hace 2 min",
            [
                new DocumentSection("chapter-1", null, "1", "Introducción", string.Empty, "En proceso", 0, 0, 1),
                new DocumentSection("section-problem", "chapter-1", "1.1", "Planteamiento del problema", "Las comunidades rurales enfrentan costos energéticos elevados y un acceso desigual a infraestructura confiable.\n\nLa energía solar comunitaria ofrece una alternativa viable cuando se acompaña de financiamiento, mantenimiento local y participación ciudadana.\n\nEste estudio analiza los factores que influyen en la adopción sostenible de estas soluciones.", "En proceso", 86, 2, 2),
                new DocumentSection("chapter-2", null, "2", "Marco teórico", string.Empty, "Borrador", 0, 0, 3),
                new DocumentSection("section-evidence", "chapter-2", "2.1", "Evidencia disponible", "Los reportes internacionales muestran un crecimiento sostenido de las energías renovables.\n\nLa evidencia local destaca que el acceso al crédito es decisivo.\n\nSe requiere evaluar las condiciones de cada territorio.", "Borrador", 54, 2, 4)
            ], [new RelatedDocumentReference("ref-iea", 94), new RelatedDocumentReference("ref-irena", 82)], ["note-barriers", "note-impact"]);
        var graph = new KnowledgeGraphSnapshot("node-project", [
            new KnowledgeNode("node-project", "Proyecto", "Energía solar comunitaria", "Proyecto", .48, .44, 1.4, "Project"),
            new KnowledgeNode("node-barriers", "Barreras", "Financiamiento y costo", "Concepto", .24, .27, 1, "Concept"),
            new KnowledgeNode("node-impact", "Impacto social", "Participación local", "Nota", .67, .26, 1, "Note"),
            new KnowledgeNode("node-iea", "IEA 2024", "Renewables 2024", "Referencia", .30, .70, .9, "Reference"),
            new KnowledgeNode("node-irena", "IRENA 2023", "Energy transitions", "Referencia", .68, .70, .9, "Reference"),
            new KnowledgeNode("node-quote", "Cita clave", "Transición justa", "Cita", .48, .82, .8, "Citation")],
            [new KnowledgeConnection("edge-1", "node-project", "node-barriers", "analiza"), new KnowledgeConnection("edge-2", "node-project", "node-impact", "incluye"), new KnowledgeConnection("edge-3", "node-barriers", "node-iea", "respaldado por"), new KnowledgeConnection("edge-4", "node-impact", "node-irena", "respaldado por"), new KnowledgeConnection("edge-5", "node-project", "node-quote", "sintetiza")],
            [new KnowledgeLegendItem("Proyecto", "Proyecto", 1, "Project"), new KnowledgeLegendItem("Concepto", "Conceptos", 1, "Concept"), new KnowledgeLegendItem("Nota", "Notas", 1, "Note"), new KnowledgeLegendItem("Referencia", "Referencias", 2, "Reference"), new KnowledgeLegendItem("Cita", "Citas", 1, "Citation")]);
        return CreateSnapshot(user, projectId,
            [new WorkspaceMetric("projects", "Proyectos activos", 1, "Investigación en curso", "Projects", "Blue"), new WorkspaceMetric("notes", "Notas", 3, "2 pendientes de revisar", "Notes", "Teal"), new WorkspaceMetric("references", "Referencias", 3, "Todas clasificadas", "References", "Petroleum"), new WorkspaceMetric("connections", "Conexiones", 5, "Mapa actualizado", "Knowledge", "Green")],
            [new WorkspaceQuickAction("note", "Nueva nota", "Captura una idea", "notes", "Notes"), new WorkspaceQuickAction("reference", "Nueva referencia", "Registra una fuente", "references", "References"), new WorkspaceQuickAction("network", "Ver red", "Explora conexiones", "knowledge", "Knowledge"), new WorkspaceQuickAction("document", "Continuar redacción", "Abre el documento", "documents", "Documents")],
            [new WorkspaceTask("task-review", "Revisar entrevistas", "Clasifica los hallazgos iniciales.", "Alta", "Hoy", false), new WorkspaceTask("task-cite", "Validar citas", "Confirma las fuentes del marco teórico.", "Media", "Esta semana", false)],
            [project], notes, [new WorkspaceActivity("activity-1", "Nota", "Nota actualizada", "Barreras de adopción", "Hace 12 min", "note-barriers", "Notes"), new WorkspaceActivity("activity-2", "Fuente", "Referencia vinculada", "Renewables 2024", "Hace 1 h", "ref-iea", "References")], references,
            [new WorkspaceCitation("citation-1", "ref-iea", "note-barriers", "La transición energética requiere soluciones accesibles y adaptadas al contexto local.", "International Energy Agency · Renewables 2024", "p. 34", "Marco teórico", ["Energía", "Transición"]), new WorkspaceCitation("citation-2", "ref-irena", "note-impact", "La participación comunitaria es esencial para una transición energética justa.", "IRENA · World Energy Transitions Outlook", "p. 72", "Impacto social", ["Comunidad", "Política"])],
            document, [new DocumentVersion("version-3", "Versión 3", true, "Hace 2 min", "Planteamiento del problema", 86, 4, "Se amplió el contexto territorial.", [new VersionChange("change-1", "Adición", "", "Se agregó el análisis de financiamiento local.")]), new DocumentVersion("version-2", "Versión 2", false, "Ayer", "Planteamiento del problema", 42, 0, "Primer borrador", [])], graph);
    }

    private static WorkspaceSnapshot CreateSnapshot(WorkspaceUser user, string activeProjectId, IReadOnlyList<WorkspaceMetric> metrics, IReadOnlyList<WorkspaceQuickAction> actions, IReadOnlyList<WorkspaceTask> tasks, IReadOnlyList<WorkspaceProject> projects, IReadOnlyList<WorkspaceNote> notes, IReadOnlyList<WorkspaceActivity> activities, IReadOnlyList<WorkspaceReference> references, IReadOnlyList<WorkspaceCitation> citations, WorkspaceDocument document, IReadOnlyList<DocumentVersion> versions, KnowledgeGraphSnapshot graph) =>
        new(user, activeProjectId, metrics, actions, tasks, projects, notes, activities, references, citations, graph, document, versions,
            [new ExportOption("pdf", "PDF", ".pdf", "Documento portátil", "Pdf", true, true), new ExportOption("docx", "Word", ".docx", "Documento editable", "Word", false, false), new ExportOption("md", "Markdown", ".md", "Texto estructurado", "Markdown", false, false)],
            new ExportSummary(document.Title, "PDF", "APA", document.Sections.Count(section => section.ParentId is null), document.Sections.Count(section => section.ParentId is null), references.Count, Math.Max(1, document.TotalWordCount / 250)));

    private static WorkspaceUser CreateUser(int userId, string role)
    {
        var firstName = AppSession.UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Usuario";
        return new WorkspaceUser(userId.ToString(), AppSession.UserName, firstName, string.Empty, role, firstName[..1].ToUpperInvariant());
    }

    private static WorkspaceDocument EmptyDocument() => new("document-empty", string.Empty, "Documento sin título", "Sin proyecto asignado", "section-1", 0, "Listo para redactar", [new DocumentSection("chapter-1", null, "1", "Documento", string.Empty, "Borrador", 0, 0, 1), new DocumentSection("section-1", "chapter-1", "1.1", "Nueva sección", string.Empty, "Borrador", 0, 0, 2)], [], []);
}

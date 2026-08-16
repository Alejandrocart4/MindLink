using MindLink.Application.Interfaces;
using MindLink.Application.Models;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Services;

public sealed class EmptyWorkspaceDataService : IWorkspaceDataService
{
    public Task<WorkspaceSnapshot> GetWorkspaceAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(AppSession.Plan == "Pro" ? CreateProWorkspace(userId) : CreateFreeWorkspace(userId));

    private static WorkspaceSnapshot CreateFreeWorkspace(int userId)
    {
        var user = CreateUser(userId, "Plan Free");
        return CreateSnapshot(user, string.Empty, [], [], [], [], [], [], [], [], EmptyDocument(), [], new KnowledgeGraphSnapshot(string.Empty, [], [], []));
    }

    private static WorkspaceSnapshot CreateProWorkspace(int userId)
    {
        var user = CreateUser(userId, "Plan Pro");
        const string projectId = "project-renewable-energy";
        var project = new WorkspaceProject(projectId, "Energia renovable comunitaria", "Energia renovable", "Analisis de adopcion de energia solar en comunidades rurales.", "Investigacion", 58, 3, 3, "Hace 12 min", ["Energia", "Desarrollo local"], ["CP", "AR"]);
        var notes = new[]
        {
            new WorkspaceNote("note-barriers", projectId, "Barreras de adopcion", "El costo inicial y la falta de financiamiento aparecen como los principales obstaculos.", "Revisada", 2, 3, "Hace 12 min", ["Financiamiento", "Barreras"]),
            new WorkspaceNote("note-impact", projectId, "Impacto social", "Las cooperativas locales fortalecen el mantenimiento y la apropiacion del proyecto.", "Borrador", 1, 2, "Ayer", ["Comunidad", "Impacto"]),
            new WorkspaceNote("note-method", projectId, "Diseno metodologico", "Combinar entrevistas semiestructuradas con una encuesta de hogares.", "Borrador", 0, 1, "Hace 2 dias", ["Metodologia"])
        };
        var references = new[]
        {
            new WorkspaceReference("ref-iea", projectId, "International Energy Agency", 2024, "Renewables 2024", "IEA", "Informe institucional", "Leido", 3, true, true, "https://www.iea.org/reports/renewables-2024", ["Energia", "Mercado"]),
            new WorkspaceReference("ref-irena", projectId, "IRENA", 2023, "World Energy Transitions Outlook", "International Renewable Energy Agency", "Informe institucional", "En lectura", 2, true, true, "https://www.irena.org/Publications", ["Transicion energetica", "Politica"]),
            new WorkspaceReference("ref-finance", projectId, "Gonzalez, M.", 2022, "Financiamiento de energia solar rural", "Revista de Desarrollo Territorial", "Articulo cientifico", "Por leer", 1, true, true, "doi:10.0000/solar-rural", ["Financiamiento", "Rural"])
        };
        var document = new WorkspaceDocument("doc-energy", projectId, "Adopcion de energia solar comunitaria", "Energia renovable comunitaria", "section-problem", 486, "Guardado hace 2 min",
            [
                new DocumentSection("chapter-1", null, "1", "Introduccion", string.Empty, "En progreso", 0, 0, 1),
                new DocumentSection("section-problem", "chapter-1", "1.1", "Planteamiento del problema", "Las comunidades rurales enfrentan costos energeticos elevados y un acceso desigual a infraestructura confiable.\n\nLa energia solar comunitaria ofrece una alternativa viable cuando se acompana de financiamiento, mantenimiento local y participacion ciudadana.\n\nEste estudio analiza los factores que influyen en la adopcion sostenible de estas soluciones.", "En progreso", 86, 2, 2),
                new DocumentSection("chapter-2", null, "2", "Marco teorico", string.Empty, "Borrador", 0, 0, 3),
                new DocumentSection("section-evidence", "chapter-2", "2.1", "Evidencia disponible", "Los reportes internacionales muestran un crecimiento sostenido de las energias renovables.\n\nLa evidencia local destaca que el acceso al credito es decisivo.\n\nSe requiere evaluar las condiciones de cada territorio.", "Borrador", 54, 2, 4)
            ], [new RelatedDocumentReference("ref-iea", 94), new RelatedDocumentReference("ref-irena", 82)], ["note-barriers", "note-impact"]);
        var graph = new KnowledgeGraphSnapshot("node-project", [
            new KnowledgeNode("node-project", "Proyecto", "Energia solar comunitaria", "Proyecto", .48, .44, 1.4, "Project"),
            new KnowledgeNode("node-barriers", "Barreras", "Financiamiento y costo", "Concepto", .24, .27, 1, "Concept"),
            new KnowledgeNode("node-impact", "Impacto social", "Participacion local", "Nota", .67, .26, 1, "Note"),
            new KnowledgeNode("node-iea", "IEA 2024", "Renewables 2024", "Referencia", .30, .70, .9, "Reference"),
            new KnowledgeNode("node-irena", "IRENA 2023", "Energy transitions", "Referencia", .68, .70, .9, "Reference"),
            new KnowledgeNode("node-quote", "Cita clave", "Transicion justa", "Cita", .48, .82, .8, "Citation")],
            [new KnowledgeConnection("edge-1", "node-project", "node-barriers", "analiza"), new KnowledgeConnection("edge-2", "node-project", "node-impact", "incluye"), new KnowledgeConnection("edge-3", "node-barriers", "node-iea", "respaldado por"), new KnowledgeConnection("edge-4", "node-impact", "node-irena", "respaldado por"), new KnowledgeConnection("edge-5", "node-project", "node-quote", "sintetiza")],
            [new KnowledgeLegendItem("Proyecto", "Proyecto", 1, "Project"), new KnowledgeLegendItem("Concepto", "Conceptos", 1, "Concept"), new KnowledgeLegendItem("Nota", "Notas", 1, "Note"), new KnowledgeLegendItem("Referencia", "Referencias", 2, "Reference"), new KnowledgeLegendItem("Cita", "Citas", 1, "Citation")]);
        return CreateSnapshot(user, projectId,
            [new WorkspaceMetric("projects", "Proyectos activos", 1, "Investigacion en curso", "Projects", "Blue"), new WorkspaceMetric("notes", "Notas", 3, "2 pendientes de revisar", "Notes", "Teal"), new WorkspaceMetric("references", "Referencias", 3, "Todas clasificadas", "References", "Petroleum"), new WorkspaceMetric("connections", "Conexiones", 5, "Mapa actualizado", "Knowledge", "Green")],
            [new WorkspaceQuickAction("note", "Nueva nota", "Captura una idea", "notes", "Notes"), new WorkspaceQuickAction("reference", "Nueva referencia", "Registra una fuente", "references", "References"), new WorkspaceQuickAction("network", "Ver red", "Explora conexiones", "knowledge", "Knowledge"), new WorkspaceQuickAction("document", "Continuar redaccion", "Abre el documento", "documents", "Documents")],
            [new WorkspaceTask("task-review", "Revisar entrevistas", "Clasifica los hallazgos iniciales.", "Alta", "Hoy", false), new WorkspaceTask("task-cite", "Validar citas", "Confirma las fuentes del marco teorico.", "Media", "Esta semana", false)],
            [project], notes, [new WorkspaceActivity("activity-1", "Nota", "Nota actualizada", "Barreras de adopcion", "Hace 12 min", "note-barriers", "Notes"), new WorkspaceActivity("activity-2", "Fuente", "Referencia vinculada", "Renewables 2024", "Hace 1 h", "ref-iea", "References")], references,
            [new WorkspaceCitation("citation-1", "ref-iea", "note-barriers", "La transicion energetica requiere soluciones accesibles y adaptadas al contexto local.", "International Energy Agency · Renewables 2024", "p. 34", "Marco teorico", ["Energia", "Transicion"]), new WorkspaceCitation("citation-2", "ref-irena", "note-impact", "La participacion comunitaria es esencial para una transicion energetica justa.", "IRENA · World Energy Transitions Outlook", "p. 72", "Impacto social", ["Comunidad", "Politica"])],
            document, [new DocumentVersion("version-3", "Version 3", true, "Hace 2 min", "Planteamiento del problema", 86, 4, "Se amplió el contexto territorial.", [new VersionChange("change-1", "Adicion", "", "Se agrego el analisis de financiamiento local.")]), new DocumentVersion("version-2", "Version 2", false, "Ayer", "Planteamiento del problema", 42, 0, "Primer borrador", [])], graph);
    }

    private static WorkspaceSnapshot CreateSnapshot(WorkspaceUser user, string activeProjectId, IReadOnlyList<WorkspaceMetric> metrics, IReadOnlyList<WorkspaceQuickAction> actions, IReadOnlyList<WorkspaceTask> tasks, IReadOnlyList<WorkspaceProject> projects, IReadOnlyList<WorkspaceNote> notes, IReadOnlyList<WorkspaceActivity> activities, IReadOnlyList<WorkspaceReference> references, IReadOnlyList<WorkspaceCitation> citations, WorkspaceDocument document, IReadOnlyList<DocumentVersion> versions, KnowledgeGraphSnapshot graph) =>
        new(user, activeProjectId, metrics, actions, tasks, projects, notes, activities, references, citations, graph, document, versions,
            [new ExportOption("pdf", "PDF", ".pdf", "Documento portatil", "Pdf", true, true), new ExportOption("docx", "Word", ".docx", "Documento editable", "Word", false, false), new ExportOption("md", "Markdown", ".md", "Texto estructurado", "Markdown", false, false)],
            new ExportSummary(document.Title, "PDF", "APA", document.Sections.Count(section => section.ParentId is null), document.Sections.Count(section => section.ParentId is null), references.Count, Math.Max(1, document.TotalWordCount / 250)));

    private static WorkspaceUser CreateUser(int userId, string role)
    {
        var firstName = AppSession.UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Usuario";
        return new WorkspaceUser(userId.ToString(), AppSession.UserName, firstName, string.Empty, role, firstName[..1].ToUpperInvariant());
    }

    private static WorkspaceDocument EmptyDocument() => new("document-empty", string.Empty, "Documento sin titulo", "Sin proyecto asignado", "section-1", 0, "Listo para redactar", [new DocumentSection("chapter-1", null, "1", "Documento", string.Empty, "Borrador", 0, 0, 1), new DocumentSection("section-1", "chapter-1", "1.1", "Nueva seccion", string.Empty, "Borrador", 0, 0, 2)], [], []);
}

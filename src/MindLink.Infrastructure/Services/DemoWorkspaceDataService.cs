using MindLink.Application.Interfaces;
using MindLink.Application.Models;

namespace MindLink.Infrastructure.Services;

public sealed class DemoWorkspaceDataService : IWorkspaceDataService
{
    private const string ActiveProjectId = "project-ai-learning";
    private static readonly WorkspaceSnapshot DemoWorkspace = CreateWorkspace();

    public Task<WorkspaceSnapshot> GetWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(DemoWorkspace);
    }

    private static WorkspaceSnapshot CreateWorkspace() => new(
        User: new WorkspaceUser(
            Id: "user-maria-rodriguez",
            FullName: "María Rodríguez",
            FirstName: "María",
            Email: "maria.rodriguez@mindlink.local",
            Role: "Investigadora",
            Initials: "MR"),
        ActiveProjectId: ActiveProjectId,
        Metrics: CreateMetrics(),
        QuickActions: CreateQuickActions(),
        PendingTasks: CreatePendingTasks(),
        Projects: CreateProjects(),
        Notes: CreateNotes(),
        Activities: CreateActivities(),
        References: CreateReferences(),
        Citations: CreateCitations(),
        KnowledgeGraph: CreateKnowledgeGraph(),
        ActiveDocument: CreateDocument(),
        Versions: CreateVersions(),
        ExportOptions: CreateExportOptions(),
        ExportSummary: new ExportSummary(
            ProjectTitle: "Impacto IA en educación",
            Format: "PDF",
            CitationStyle: "APA 7ª ed.",
            IncludedSections: 3,
            TotalSections: 4,
            IncludedReferences: 83,
            EstimatedPages: 28));

    private static IReadOnlyList<WorkspaceMetric> CreateMetrics() =>
    [
        new("metric-projects", "Proyectos activos", 4, "+1 esta semana", "Projects", "Blue"),
        new("metric-notes", "Notas creadas", 127, "+12 esta semana", "Notes", "Teal"),
        new("metric-references", "Referencias guardadas", 83, "6 sin clasificar", "References", "Violet"),
        new("metric-connections", "Relaciones entre ideas", 241, "+28 esta semana", "Connections", "Cyan")
    ];

    private static IReadOnlyList<WorkspaceQuickAction> CreateQuickActions() =>
    [
        new("action-new-note", "Nueva nota", "Captura una idea sin perder el contexto.", "notes/new", "AddNote"),
        new("action-new-reference", "Nueva referencia", "Registra una fuente para tu investigación.", "references/new", "AddReference"),
        new("action-new-connection", "Crear relación", "Conecta dos elementos de conocimiento.", "knowledge/new-connection", "AddConnection"),
        new("action-import-document", "Importar documento", "Integra un documento a tu espacio local.", "documents/import", "ImportDocument")
    ];

    private static IReadOnlyList<WorkspaceTask> CreatePendingTasks() =>
    [
        new("task-classify-references", "Clasificar referencias pendientes", "Revisa las 2 referencias incompletas del proyecto activo.", "Alta", "Hoy", false),
        new("task-review-section", "Revisar la sección 2.3", "Valida la evidencia de Personalización educativa mediada por IA.", "Media", "Hoy", false),
        new("task-link-citations", "Vincular citas al marco ético", "Relaciona la evidencia de Selwyn con la sección 2.4.", "Media", "Mañana", false)
    ];

    private static IReadOnlyList<WorkspaceProject> CreateProjects() =>
    [
        new(
            Id: ActiveProjectId,
            Title: "Impacto de la inteligencia artificial en el aprendizaje universitario",
            ShortTitle: "Impacto IA en educación",
            Description: "Análisis del rol de la IA en la personalización educativa y sus implicaciones éticas en educación superior latinoamericana.",
            Status: "Redacción",
            Progress: 65,
            NotesCount: 127,
            ReferencesCount: 83,
            UpdatedLabel: "Hoy 14:32",
            Tags: ["Inteligencia artificial", "Educación superior"],
            CollaboratorInitials: ["MR", "JP"]),
        new(
            Id: "project-algorithmic-ethics",
            Title: "Ética algorítmica en sistemas de evaluación automatizada",
            ShortTitle: "Ética algorítmica en evaluación",
            Description: "Revisión sistemática sobre sesgos en algoritmos de calificación y su impacto en la equidad educativa.",
            Status: "Investigación",
            Progress: 34,
            NotesCount: 48,
            ReferencesCount: 51,
            UpdatedLabel: "Ayer",
            Tags: ["Ética", "Algoritmos"],
            CollaboratorInitials: ["MR"]),
        new(
            Id: "project-student-dropout",
            Title: "Modelos predictivos de abandono estudiantil universitario",
            ShortTitle: "Predicción de abandono estudiantil",
            Description: "Propuesta metodológica para identificar factores de riesgo de deserción mediante machine learning.",
            Status: "Idea",
            Progress: 10,
            NotesCount: 12,
            ReferencesCount: 9,
            UpdatedLabel: "Hace 3 días",
            Tags: ["Machine learning", "Deserción"],
            CollaboratorInitials: ["MR", "AM", "JP"]),
        new(
            Id: "project-digital-competencies",
            Title: "Evaluación de competencias digitales docentes post-pandemia",
            ShortTitle: "Competencias digitales docentes",
            Description: "Instrumentos de medición y propuestas de formación para docentes universitarios en entornos híbridos.",
            Status: "Revisión",
            Progress: 88,
            NotesCount: 94,
            ReferencesCount: 67,
            UpdatedLabel: "Hace 1 semana",
            Tags: ["Competencias digitales", "Docentes"],
            CollaboratorInitials: ["MR", "GL"]),
        new(
            Id: "project-stem-gamification",
            Title: "Gamificación en educación STEM universitaria",
            ShortTitle: "Gamificación en STEM",
            Description: "Meta-análisis del impacto de elementos de juego en la motivación y rendimiento académico en STEM.",
            Status: "Finalizado",
            Progress: 100,
            NotesCount: 76,
            ReferencesCount: 58,
            UpdatedLabel: "Hace 2 semanas",
            Tags: ["Gamificación", "STEM"],
            CollaboratorInitials: ["MR", "JP"])
    ];

    private static IReadOnlyList<WorkspaceNote> CreateNotes() =>
    [
        new(
            Id: "note-adaptive-learning",
            ProjectId: ActiveProjectId,
            Title: "Aprendizaje adaptativo — características clave",
            Excerpt: "Los sistemas adaptativos ajustan contenido, ritmo y retroalimentación a partir del desempeño y el contexto de cada estudiante.",
            Status: "Revisada",
            ReferencesCount: 3,
            ConnectionsCount: 4,
            UpdatedLabel: "Hoy 14:32",
            Tags: ["IA", "Aprendizaje", "LMS"]),
        new(
            Id: "note-automated-feedback",
            ProjectId: ActiveProjectId,
            Title: "Retroalimentación automatizada en plataformas LMS",
            Excerpt: "La retroalimentación inmediata puede orientar la autorregulación cuando explica el error y propone una acción siguiente.",
            Status: "Borrador",
            ReferencesCount: 2,
            ConnectionsCount: 3,
            UpdatedLabel: "Hoy 11:15",
            Tags: ["LMS", "Retroalimentación", "Tecnología"]),
        new(
            Id: "note-technology-dependence",
            ProjectId: ActiveProjectId,
            Title: "Riesgos de dependencia tecnológica en educación",
            Excerpt: "Delegar decisiones pedagógicas a plataformas opacas puede reducir la autonomía docente y ampliar desigualdades existentes.",
            Status: "Revisada",
            ReferencesCount: 4,
            ConnectionsCount: 2,
            UpdatedLabel: "Ayer 18:30",
            Tags: ["Crítica", "Ética", "Tecnología"]),
        new(
            Id: "note-personalization-models",
            ProjectId: ActiveProjectId,
            Title: "Personalización educativa — definiciones y modelos",
            Excerpt: "La personalización combina objetivos comunes con rutas diferenciadas según conocimientos previos, necesidades y motivaciones.",
            Status: "Revisada",
            ReferencesCount: 5,
            ConnectionsCount: 5,
            UpdatedLabel: "Ayer 10:00",
            Tags: ["Personalización", "Educación", "Modelos"]),
        new(
            Id: "note-algorithmic-bias",
            ProjectId: ActiveProjectId,
            Title: "Sesgos en algoritmos educativos",
            Excerpt: "Los datos históricos pueden reproducir patrones de exclusión cuando no se auditan las variables y decisiones del modelo.",
            Status: "Borrador",
            ReferencesCount: 3,
            ConnectionsCount: 2,
            UpdatedLabel: "Hace 2 días",
            Tags: ["Sesgos", "Ética", "Algoritmos"]),
        new(
            Id: "note-ai-ethics-framework",
            ProjectId: ActiveProjectId,
            Title: "Marco ético para sistemas de IA educativa",
            Excerpt: "Transparencia, supervisión humana, privacidad y equidad forman una base mínima para evaluar sistemas educativos con IA.",
            Status: "Revisada",
            ReferencesCount: 6,
            ConnectionsCount: 4,
            UpdatedLabel: "Hace 3 días",
            Tags: ["Ética", "Marco normativo", "IA"])
    ];

    private static IReadOnlyList<WorkspaceActivity> CreateActivities() =>
    [
        new("activity-note-edited", "Nota editada", "Nota editada", "Retroalimentación automatizada en LMS", "Hace 12 min", "note-automated-feedback", "EditNote"),
        new("activity-reference-added", "Referencia agregada", "Referencia agregada", "UNESCO (2023). AI in Education Report", "Hace 1 hora", "reference-unesco", "AddReference"),
        new("activity-connection-created", "Nueva relación", "Nueva relación", "Aprendizaje adaptativo → Personalización educativa", "Hace 2 horas", "connection-adaptive-personalization", "AddConnection"),
        new("activity-document-updated", "Documento actualizado", "Documento actualizado", "Marco Teórico · Sección 2.3", "Ayer 18:30", "section-2-3", "Document")
    ];

    private static IReadOnlyList<WorkspaceReference> CreateReferences() =>
    [
        new(
            Id: "reference-selwyn",
            ProjectId: ActiveProjectId,
            Authors: "Selwyn, N.",
            Year: 2022,
            Title: "Distrusting Educational Technology: Critical Questions for Changing Times",
            Source: "Routledge",
            ReferenceType: "Libro",
            ReadingStatus: "Leído",
            UsageCount: 12,
            IsComplete: true,
            IsClassified: false,
            Identifier: "ISBN 978-0-367-26038-9",
            Tags: ["Crítica", "Tecnología educativa", "Ética"]),
        new(
            Id: "reference-unesco",
            ProjectId: ActiveProjectId,
            Authors: "UNESCO",
            Year: 2023,
            Title: "AI Competency Framework for Students",
            Source: "UNESCO",
            ReferenceType: "Informe institucional",
            ReadingStatus: "En lectura",
            UsageCount: 8,
            IsComplete: false,
            IsClassified: false,
            Identifier: string.Empty,
            Tags: ["IA", "Competencias", "Educación"]),
        new(
            Id: "reference-holmes",
            ProjectId: ActiveProjectId,
            Authors: "Holmes, W., Porayska-Pomsta, K. et al.",
            Year: 2021,
            Title: "Ethics of AI in Education: Towards a Community-Wide Agenda",
            Source: "International Journal of Artificial Intelligence in Education",
            ReferenceType: "Artículo científico",
            ReadingStatus: "Leído",
            UsageCount: 7,
            IsComplete: true,
            IsClassified: false,
            Identifier: "https://doi.org/10.1007/s40593-021-00239-1",
            Tags: ["Ética", "IA educativa", "Agenda de investigación"]),
        new(
            Id: "reference-zawacki-richter",
            ProjectId: ActiveProjectId,
            Authors: "Zawacki-Richter, O. et al.",
            Year: 2019,
            Title: "Systematic review of research on artificial intelligence applications in higher education — where are the educators?",
            Source: "International Journal of Educational Technology in Higher Education",
            ReferenceType: "Revisión sistemática",
            ReadingStatus: "Por leer",
            UsageCount: 3,
            IsComplete: false,
            IsClassified: false,
            Identifier: string.Empty,
            Tags: ["Educación superior", "Revisión sistemática", "IA"]),
        new(
            Id: "reference-fernandez-batanero",
            ProjectId: ActiveProjectId,
            Authors: "Fernández-Batanero, J. M. et al.",
            Year: 2022,
            Title: "Uso de la inteligencia artificial en la educación universitaria: revisión sistemática",
            Source: "Revista de Educación a Distancia",
            ReferenceType: "Artículo científico",
            ReadingStatus: "Por leer",
            UsageCount: 2,
            IsComplete: true,
            IsClassified: false,
            Identifier: "https://doi.org/10.6018/red.511041",
            Tags: ["Educación universitaria", "IA", "Revisión"]),
        new(
            Id: "reference-luckin",
            ProjectId: ActiveProjectId,
            Authors: "Luckin, R. et al.",
            Year: 2016,
            Title: "Intelligence Unleashed: An Argument for AI in Education",
            Source: "Pearson",
            ReferenceType: "Informe institucional",
            ReadingStatus: "Leído",
            UsageCount: 5,
            IsComplete: true,
            IsClassified: false,
            Identifier: "ISBN 978-0-992-42591-9",
            Tags: ["IA educativa", "Aprendizaje", "Innovación"])
    ];

    private static IReadOnlyList<WorkspaceCitation> CreateCitations() =>
    [
        new(
            Id: "citation-selwyn-power",
            ReferenceId: "reference-selwyn",
            NoteId: "note-technology-dependence",
            Quote: "La promesa de la personalización algorítmica no puede disociarse de las estructuras de poder que determinan qué datos se recogen y cómo se interpretan.",
            SourceLabel: "Selwyn, N. (2022) · Distrusting Educational Technology",
            PageLabel: "p. 87",
            Context: "Crítica al determinismo tecnológico en educación",
            Tags: ["Crítica", "IA", "Poder"]),
        new(
            Id: "citation-unesco-adaptive",
            ReferenceId: "reference-unesco",
            NoteId: "note-adaptive-learning",
            Quote: "Los sistemas de aprendizaje adaptativo de nueva generación no solo responden al rendimiento académico, sino que integran dimensiones afectivas, contextuales y motivacionales.",
            SourceLabel: "UNESCO (2023) · AI Competency Framework for Students",
            PageLabel: "p. 34",
            Context: "Definición operacional de aprendizaje adaptativo",
            Tags: ["Aprendizaje adaptativo", "IA", "Afecto"]),
        new(
            Id: "citation-holmes-ethics",
            ReferenceId: "reference-holmes",
            NoteId: "note-ai-ethics-framework",
            Quote: "Ethical issues in AI and education are not merely technical problems. They require interdisciplinary collaboration between educators, technologists, and policy makers.",
            SourceLabel: "Holmes, W. et al. (2021) · Ethics of AI in Education",
            PageLabel: "p. 12",
            Context: "Marco ético para IA en educación",
            Tags: ["Ética", "Interdisciplinario"]),
        new(
            Id: "citation-luckin-prediction",
            ReferenceId: "reference-luckin",
            NoteId: "note-adaptive-learning",
            Quote: "Machine learning systems can potentially identify students at risk of academic failure weeks or even months before conventional assessment methods.",
            SourceLabel: "Luckin, R. (2018) · Machine Learning and Human Intelligence",
            PageLabel: "p. 142",
            Context: "Capacidades predictivas de ML en educación",
            Tags: ["Predicción", "Machine learning"])
    ];

    private static KnowledgeGraphSnapshot CreateKnowledgeGraph() => new(
        SelectedNodeId: "node-adaptive-ai",
        Nodes:
        [
            new("node-project-ai", "Proyecto: IA", "Impacto de la IA en educación", "Proyecto", 0.10, 0.48, 1.25, "Project"),
            new("node-adaptive-ai", "IA Adaptativa", "Concepto seleccionado", "Concepto", 0.43, 0.43, 1.35, "Concept"),
            new("node-personalized-learning", "Aprendizaje personalizado", "Concepto", "Concepto", 0.38, 0.14, 1.00, "Concept"),
            new("node-education-personalization", "Personalización educativa", "Concepto", "Concepto", 0.69, 0.18, 1.05, "Concept"),
            new("node-automated-feedback", "Retroalimentación automática", "Concepto", "Concepto", 0.72, 0.47, 1.00, "Concept"),
            new("node-algorithmic-ethics", "Ética algorítmica", "Concepto", "Concepto", 0.54, 0.78, 1.05, "Concept"),
            new("node-unesco", "UNESCO (2023)", "Referencia", "Referencia", 0.25, 0.20, 0.92, "Reference"),
            new("node-selwyn", "Selwyn (2022)", "Referencia", "Referencia", 0.28, 0.77, 0.92, "Reference"),
            new("node-lms", "Plataformas LMS", "Nota", "Nota", 0.86, 0.63, 0.88, "Note"),
            new("node-bias", "Sesgos en algoritmos", "Nota", "Nota", 0.73, 0.88, 0.88, "Note"),
            new("node-risks", "Riesgos de dependencia", "Nota", "Nota", 0.30, 0.93, 0.88, "Note"),
            new("node-power-quote", "Cita: poder", "Cita", "Cita", 0.11, 0.78, 0.76, "Citation")
        ],
        Connections:
        [
            new("connection-project-adaptive", "node-project-ai", "node-adaptive-ai", "incluye"),
            new("connection-adaptive-learning", "node-adaptive-ai", "node-automated-feedback", "relacionado"),
            new("connection-adaptive-personalization", "node-personalized-learning", "node-education-personalization", "expande"),
            new("connection-adaptive-lms", "node-adaptive-ai", "node-lms", "implementado en"),
            new("connection-unesco-adaptive", "node-unesco", "node-adaptive-ai", "define"),
            new("connection-selwyn-ethics", "node-selwyn", "node-algorithmic-ethics", "fundamentado en"),
            new("connection-quote-selwyn", "node-power-quote", "node-selwyn", "citado en"),
            new("connection-risks-ethics", "node-risks", "node-algorithmic-ethics", "expande"),
            new("connection-project-ethics", "node-project-ai", "node-algorithmic-ethics", "investiga"),
            new("connection-ethics-bias", "node-algorithmic-ethics", "node-bias", "genera"),
            new("connection-lms-feedback", "node-lms", "node-automated-feedback", "contiene"),
            new("connection-unesco-personalized", "node-unesco", "node-personalized-learning", "apoya"),
            new("connection-selwyn-risks", "node-selwyn", "node-risks", "apoya")
        ],
        Legend:
        [
            new("Proyecto", "Proyecto", 1, "Project"),
            new("Concepto", "Concepto", 5, "Concept"),
            new("Referencia", "Referencia", 2, "Reference"),
            new("Nota", "Nota", 3, "Note"),
            new("Cita", "Cita", 1, "Citation")
        ]);

    private static WorkspaceDocument CreateDocument() => new(
        Id: "document-main-draft",
        ProjectId: ActiveProjectId,
        Title: "Impacto de la inteligencia artificial en el aprendizaje universitario",
        ContextLabel: "CAPÍTULO 2 · SECCIÓN 2.3",
        SelectedSectionId: "section-2-3",
        TotalWordCount: 2847,
        LastSavedLabel: "Guardado hace 2 min",
        Sections:
        [
            new("chapter-1", null, "1", "Introducción", string.Empty, "Completado", 2340, 8, 1),
            new("section-1-1", "chapter-1", "1.1", "Planteamiento del problema", string.Empty, "Completado", 920, 3, 2),
            new("section-1-2", "chapter-1", "1.2", "Justificación", string.Empty, "Completado", 710, 2, 3),
            new("section-1-3", "chapter-1", "1.3", "Objetivos", string.Empty, "Completado", 710, 3, 4),
            new("chapter-2", null, "2", "Marco Teórico", string.Empty, "En redacción", 6840, 26, 5),
            new("section-2-1", "chapter-2", "2.1", "Inteligencia artificial educativa", string.Empty, "Revisado", 1480, 7, 6),
            new("section-2-2", "chapter-2", "2.2", "Aprendizaje adaptativo", string.Empty, "Revisado", 1375, 6, 7),
            new(
                Id: "section-2-3",
                ParentId: "chapter-2",
                Number: "2.3",
                Title: "Personalización educativa mediada por IA",
                Content: """
                    La personalización educativa mediada por inteligencia artificial comprende el uso de datos y modelos computacionales para adaptar contenidos, secuencias y formas de retroalimentación a las necesidades de cada estudiante. A diferencia de una simple automatización, esta perspectiva busca interpretar el progreso dentro de su contexto y ofrecer rutas de aprendizaje relevantes.

                    En las plataformas LMS, los sistemas adaptativos pueden identificar patrones de desempeño y proponer actividades con distintos niveles de complejidad. La retroalimentación automática aporta valor cuando explica el error, conserva la trazabilidad de la decisión y permite que docentes y estudiantes intervengan sobre la recomendación.

                    Sin embargo, la promesa de personalización debe analizarse junto con sus implicaciones éticas. La calidad de los datos, la transparencia del modelo y la supervisión humana determinan si estas herramientas amplían oportunidades o reproducen sesgos existentes. Por ello, la IA educativa debe entenderse como apoyo al criterio pedagógico y no como sustituto de la relación educativa.
                    """,
                Status: "En redacción",
                WordCount: 2847,
                EvidenceCount: 9,
                Order: 8),
            new("section-2-4", "chapter-2", "2.4", "Implicaciones éticas", string.Empty, "Borrador", 1138, 4, 9),
            new("chapter-3", null, "3", "Metodología", string.Empty, "Estructurado", 920, 3, 10),
            new("section-3-1", "chapter-3", "3.1", "Enfoque de investigación", string.Empty, "Estructurado", 480, 2, 11),
            new("section-3-2", "chapter-3", "3.2", "Diseño metodológico", string.Empty, "Pendiente", 440, 1, 12)
        ],
        RelatedReferences:
        [
            new("reference-selwyn", 95),
            new("reference-unesco", 88),
            new("reference-holmes", 82)
        ],
        ConnectedNoteIds:
        [
            "note-adaptive-learning",
            "note-automated-feedback",
            "note-technology-dependence"
        ]);

    private static IReadOnlyList<DocumentVersion> CreateVersions() =>
    [
        new(
            Id: "version-4",
            Label: "v4",
            IsCurrent: true,
            TimeLabel: "Hoy 14:32",
            SectionLabel: "Marco Teórico 2.3",
            AddedWords: 342,
            RemovedWords: 28,
            Summary: "Expansión sobre personalización y retroalimentación automática",
            Changes:
            [
                new(
                    Id: "change-v4-personalization",
                    ChangeType: "Modificación",
                    PreviousText: "Agregado análisis de plataformas LMS",
                    CurrentText: "Expansión sobre personalización y retroalimentación automática")
            ]),
        new(
            Id: "version-3",
            Label: "v3",
            IsCurrent: false,
            TimeLabel: "Hoy 09:15",
            SectionLabel: "Marco Teórico 2.2",
            AddedWords: 180,
            RemovedWords: 5,
            Summary: "Agregado análisis de plataformas LMS",
            Changes: []),
        new(
            Id: "version-2",
            Label: "v2",
            IsCurrent: false,
            TimeLabel: "Ayer 18:30",
            SectionLabel: "Introducción general",
            AddedWords: 512,
            RemovedWords: 44,
            Summary: "Revisión del planteamiento y la justificación",
            Changes: []),
        new(
            Id: "version-1",
            Label: "v1",
            IsCurrent: false,
            TimeLabel: "Hace 3 días",
            SectionLabel: "Estructura inicial",
            AddedWords: 850,
            RemovedWords: 0,
            Summary: "Creación de la estructura inicial del documento",
            Changes: [])
    ];

    private static IReadOnlyList<ExportOption> CreateExportOptions() =>
    [
        new("export-pdf", "PDF", ".pdf", "Recomendado para entrega y presentación.", "Pdf", true, true),
        new("export-word", "Word", ".docx", "Ideal para edición colaborativa posterior.", "Word", false, false),
        new("export-markdown", "Markdown", ".md", "Preparado para publicación web o repositorios.", "Markdown", false, false),
        new("export-text", "Texto plano", ".txt", "Formato universal sin estilo.", "Text", false, false)
    ];
}

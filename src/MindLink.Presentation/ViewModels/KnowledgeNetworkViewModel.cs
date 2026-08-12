using System.Collections.ObjectModel;
using System.Windows.Input;
using MindLink.Application.Models;

namespace MindLink.Presentation.ViewModels;

public sealed class KnowledgeNetworkViewModel : ObservableObject
{
    private const double ZoomStep = 0.15;
    private KnowledgeNodeItemViewModel? selectedNode;
    private string activeView = "Red";
    private double zoom = 1;
    private string statusMessage = string.Empty;

    public KnowledgeNetworkViewModel(WorkspaceSnapshot workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        Nodes = new ObservableCollection<KnowledgeNodeItemViewModel>(
            workspace.KnowledgeGraph.Nodes.Select(node => new KnowledgeNodeItemViewModel(node)));
        Connections = new ObservableCollection<KnowledgeEdgeItemViewModel>(
            workspace.KnowledgeGraph.Connections.Select(connection => CreateEdge(connection)));
        Legend = new ObservableCollection<KnowledgeLegendItemViewModel>(
            workspace.KnowledgeGraph.Legend.Select(item => new KnowledgeLegendItemViewModel(item)));
        Filters =
        [
            new("Todos los tipos", "All", true),
            new("Solo conceptos", "Concepto", false),
            new("Solo referencias", "Referencia", false),
            new("Solo notas", "Nota", false)
        ];

        SelectNodeCommand = new RelayCommand<KnowledgeNodeItemViewModel>(SelectNode);
        ShowNetworkCommand = new RelayCommand(() => ActiveView = "Red");
        ShowListCommand = new RelayCommand(() => ActiveView = "Lista");
        ZoomInCommand = new RelayCommand(() => Zoom = Math.Min(2, Zoom + ZoomStep));
        ZoomOutCommand = new RelayCommand(() => Zoom = Math.Max(0.5, Zoom - ZoomStep));
        ResetZoomCommand = new RelayCommand(() => Zoom = 1);
        ClearSelectionCommand = new RelayCommand(() => SelectNode(null));
        CreateRelationshipCommand = new RelayCommand(CreateRelationship, () => SelectedNode is not null);
        SetFilterCommand = new RelayCommand<NetworkFilterOptionViewModel>(SetFilter);

        SelectNode(Nodes.FirstOrDefault(node => node.Id == workspace.KnowledgeGraph.SelectedNodeId));
    }

    public ObservableCollection<KnowledgeNodeItemViewModel> Nodes { get; }
    public ObservableCollection<KnowledgeEdgeItemViewModel> Connections { get; }
    public ObservableCollection<KnowledgeLegendItemViewModel> Legend { get; }
    public ObservableCollection<NetworkFilterOptionViewModel> Filters { get; }
    public ObservableCollection<KnowledgeConnectionDetailViewModel> SelectedConnections { get; } = [];

    public ICommand SelectNodeCommand { get; }
    public ICommand ShowNetworkCommand { get; }
    public ICommand ShowListCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public RelayCommand CreateRelationshipCommand { get; }
    public ICommand ClearSelectionCommand { get; }
    public ICommand SetFilterCommand { get; }

    public KnowledgeNodeItemViewModel? SelectedNode
    {
        get => selectedNode;
        private set
        {
            if (!SetProperty(ref selectedNode, value)) return;
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    public string ActiveView
    {
        get => activeView;
        private set
        {
            if (!SetProperty(ref activeView, value)) return;
            OnPropertyChanged(nameof(IsNetworkView));
            OnPropertyChanged(nameof(IsListView));
        }
    }

    public double Zoom
    {
        get => zoom;
        private set
        {
            if (!SetProperty(ref zoom, Math.Round(value, 2))) return;
            OnPropertyChanged(nameof(ZoomLabel));
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool HasSelection => SelectedNode is not null;
    public bool IsNetworkView => ActiveView == "Red";
    public bool IsListView => ActiveView == "Lista";
    public string ZoomLabel => $"{Zoom:P0}";
    public int NodeCount => Nodes.Count;
    public int ConnectionCount => Connections.Count;

    private KnowledgeEdgeItemViewModel CreateEdge(KnowledgeConnection connection)
    {
        var source = Nodes.First(node => node.Id == connection.SourceNodeId);
        var target = Nodes.First(node => node.Id == connection.TargetNodeId);
        return new KnowledgeEdgeItemViewModel(connection.Id, source, target, connection.Label);
    }

    private void SelectNode(KnowledgeNodeItemViewModel? node)
    {
        SelectedNode = ReferenceEquals(SelectedNode, node) ? null : node;
        StatusMessage = string.Empty;
        RefreshSelection();
        CreateRelationshipCommand.RaiseCanExecuteChanged();
    }

    private void RefreshSelection()
    {
        SelectedConnections.Clear();
        foreach (var node in Nodes)
        {
            node.IsSelected = ReferenceEquals(node, SelectedNode);
            node.IsConnected = SelectedNode is null || Connections.Any(edge =>
                (ReferenceEquals(edge.Source, SelectedNode) && ReferenceEquals(edge.Target, node)) ||
                (ReferenceEquals(edge.Target, SelectedNode) && ReferenceEquals(edge.Source, node)));
            node.IsDimmed = SelectedNode is not null && !node.IsSelected && !node.IsConnected;
        }

        foreach (var edge in Connections)
        {
            edge.IsHighlighted = SelectedNode is not null &&
                (ReferenceEquals(edge.Source, SelectedNode) || ReferenceEquals(edge.Target, SelectedNode));

            if (edge.IsHighlighted && SelectedNode is not null)
            {
                var other = ReferenceEquals(edge.Source, SelectedNode) ? edge.Target : edge.Source;
                SelectedConnections.Add(new KnowledgeConnectionDetailViewModel(other, edge.Label));
            }
        }
    }

    private void CreateRelationship()
    {
        if (SelectedNode is null) return;

        var candidate = Nodes.FirstOrDefault(node => !ReferenceEquals(node, SelectedNode) && !Connections.Any(edge =>
            (ReferenceEquals(edge.Source, SelectedNode) && ReferenceEquals(edge.Target, node)) ||
            (ReferenceEquals(edge.Target, SelectedNode) && ReferenceEquals(edge.Source, node))));

        if (candidate is null)
        {
            StatusMessage = "Este nodo ya está conectado con todos los elementos visibles.";
            return;
        }

        Connections.Add(new KnowledgeEdgeItemViewModel(
            $"manual-{SelectedNode.Id}-{candidate.Id}", SelectedNode, candidate, "relacionado"));
        StatusMessage = $"Relación creada con {candidate.Label}.";
        OnPropertyChanged(nameof(ConnectionCount));
        RefreshSelection();
    }

    private void SetFilter(NetworkFilterOptionViewModel? selectedFilter)
    {
        if (selectedFilter is null) return;

        foreach (var filter in Filters)
        {
            filter.IsSelected = ReferenceEquals(filter, selectedFilter);
        }

        foreach (var node in Nodes)
        {
            node.IsFilterHidden = selectedFilter.Key != "All" && node.KindLabel != selectedFilter.Key;
        }

        foreach (var edge in Connections)
        {
            edge.IsFilterHidden = edge.Source.IsFilterHidden || edge.Target.IsFilterHidden;
        }

        StatusMessage = selectedFilter.Key == "All"
            ? "Se muestran todos los tipos de conocimiento."
            : $"Filtro activo: {selectedFilter.Label.ToLowerInvariant()}.";
    }
}

public sealed class KnowledgeNodeItemViewModel : ObservableObject
{
    private bool isSelected;
    private bool isConnected;
    private bool isDimmed;
    private bool isFilterHidden;

    public KnowledgeNodeItemViewModel(KnowledgeNode node)
    {
        Id = node.Id;
        Label = node.Label;
        Subtitle = node.Subtitle;
        Kind = node.Kind;
        AccentKey = node.AccentKey;
        (X, Y, Diameter) = ResolveLayout(node);
        AccentColor = ResolveAccent(node);
        KindLabel = ResolveKind(node);
        ShortLabel = BuildShortLabel(node.Label);
    }

    public string Id { get; }
    public string Label { get; }
    public string ShortLabel { get; }
    public string Subtitle { get; }
    public string Kind { get; }
    public string KindLabel { get; }
    public string AccentKey { get; }
    public string AccentColor { get; }
    public double X { get; }
    public double Y { get; }
    public double Diameter { get; }
    public double Left => X - Diameter / 2;
    public double Top => Y - Diameter / 2;
    public double HaloLeft => X - Diameter / 2 - 12;
    public double HaloTop => Y - Diameter / 2 - 12;
    public double HaloDiameter => Diameter + 24;

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public bool IsConnected
    {
        get => isConnected;
        set => SetProperty(ref isConnected, value);
    }

    public bool IsDimmed
    {
        get => isDimmed;
        set
        {
            if (!SetProperty(ref isDimmed, value)) return;
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public bool IsFilterHidden
    {
        get => isFilterHidden;
        set
        {
            if (!SetProperty(ref isFilterHidden, value)) return;
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public double Opacity => IsFilterHidden ? 0.08 : IsDimmed ? 0.33 : 1;

    private static string ResolveAccent(KnowledgeNode node) => node.Id switch
    {
        "node-project-ai" => "#003280",
        "node-adaptive-ai" or "node-personalized-learning" => "#006293",
        "node-education-personalization" or "node-algorithmic-ethics" or "node-lms" or "node-bias" => "#007F9D",
        "node-unesco" or "node-selwyn" => "#009BA9",
        "node-automated-feedback" or "node-risks" => "#52BE80",
        "node-power-quote" => "#E9AD38",
        _ => node.AccentKey switch
        {
            "Project" => "#003280",
            "Concept" => "#006293",
            "Reference" => "#009BA9",
            "Note" => "#52BE80",
            "Citation" => "#E9AD38",
            _ => "#006293"
        }
    };

    private static (double X, double Y, double Diameter) ResolveLayout(KnowledgeNode node) => node.Id switch
    {
        "node-project-ai" => (380, 200, 76),
        "node-adaptive-ai" => (230, 110, 56),
        "node-personalized-learning" => (540, 120, 60),
        "node-algorithmic-ethics" => (170, 270, 48),
        "node-lms" => (320, 310, 44),
        "node-selwyn" => (530, 300, 40),
        "node-unesco" => (600, 220, 40),
        "node-automated-feedback" => (120, 160, 36),
        "node-risks" => (440, 340, 36),
        "node-power-quote" => (280, 370, 32),
        "node-education-personalization" => (650, 150, 36),
        "node-bias" => (100, 330, 32),
        _ => (node.X * 700, node.Y * 420, Math.Clamp(node.Size * 48, 34, 78))
    };

    private static string ResolveKind(KnowledgeNode node) => node.Id switch
    {
        "node-lms" or "node-bias" => "Concepto",
        "node-automated-feedback" or "node-risks" => "Nota",
        _ => node.Kind
    };

    private static string BuildShortLabel(string value)
    {
        if (value.StartsWith("Proyecto:", StringComparison.OrdinalIgnoreCase)) return "Proyecto: IA";
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length <= 2 ? value : string.Join(' ', words.Take(2));
    }
}

public sealed class KnowledgeEdgeItemViewModel : ObservableObject
{
    private bool isHighlighted;
    private bool isFilterHidden;

    public KnowledgeEdgeItemViewModel(string id, KnowledgeNodeItemViewModel source, KnowledgeNodeItemViewModel target, string label)
    {
        Id = id;
        Source = source;
        Target = target;
        Label = label;
    }

    public string Id { get; }
    public KnowledgeNodeItemViewModel Source { get; }
    public KnowledgeNodeItemViewModel Target { get; }
    public string Label { get; }
    public double X1 => Source.X;
    public double Y1 => Source.Y;
    public double X2 => Target.X;
    public double Y2 => Target.Y;
    public double LabelX => (X1 + X2) / 2 - 34;
    public double LabelY => (Y1 + Y2) / 2 - 15;

    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            if (!SetProperty(ref isHighlighted, value)) return;
            OnPropertyChanged(nameof(Stroke));
            OnPropertyChanged(nameof(StrokeThickness));
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public bool IsFilterHidden
    {
        get => isFilterHidden;
        set
        {
            if (!SetProperty(ref isFilterHidden, value)) return;
            OnPropertyChanged(nameof(Opacity));
        }
    }

    public string Stroke => IsHighlighted ? "#009BA9" : "#8FC5D8";
    public double StrokeThickness => IsHighlighted ? 2.4 : 1.3;
    public double Opacity => IsFilterHidden ? 0.08 : IsHighlighted ? 1 : 0.52;
}

public sealed class NetworkFilterOptionViewModel : ObservableObject
{
    private bool isSelected;

    public NetworkFilterOptionViewModel(string label, string key, bool isSelected)
    {
        Label = label;
        Key = key;
        this.isSelected = isSelected;
    }

    public string Label { get; }
    public string Key { get; }

    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }
}

public sealed class KnowledgeLegendItemViewModel
{
    public KnowledgeLegendItemViewModel(KnowledgeLegendItem item)
    {
        Label = item.Label;
        Count = item.Count;
        AccentColor = item.AccentKey switch
        {
            "Project" => "#003280",
            "Concept" => "#006293",
            "Reference" => "#009BA9",
            "Note" => "#52BE80",
            "Citation" => "#E9AD38",
            _ => "#006293"
        };
    }

    public string Label { get; }
    public int Count { get; }
    public string AccentColor { get; }
}

public sealed class KnowledgeConnectionDetailViewModel(KnowledgeNodeItemViewModel node, string relation)
{
    public KnowledgeNodeItemViewModel Node { get; } = node;
    public string Relation { get; } = relation;
}

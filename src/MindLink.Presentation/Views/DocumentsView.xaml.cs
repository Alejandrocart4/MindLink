using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Views;

public partial class DocumentsView : UserControl
{
    private bool isLoadingEditor;
    private DocumentsViewModel? viewModel;

    public DocumentsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (viewModel is not null) viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        viewModel = e.NewValue as DocumentsViewModel;
        if (viewModel is not null) viewModel.PropertyChanged += OnViewModelPropertyChanged;
        LoadEditor();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DocumentsViewModel.SelectedSection))
            LoadEditor();
    }

    private void LoadEditor()
    {
        if (viewModel is null || DocumentEditor is null) return;
        isLoadingEditor = true;
        try
        {
            var range = new TextRange(DocumentEditor.Document.ContentStart, DocumentEditor.Document.ContentEnd);
            if (!string.IsNullOrWhiteSpace(viewModel.EditorRichText))
            {
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(viewModel.EditorRichText));
                range.Load(stream, DataFormats.Rtf);
            }
            else range.Text = viewModel.EditorPlainText;
        }
        catch
        {
            DocumentEditor.Document.Blocks.Clear();
            DocumentEditor.Document.Blocks.Add(new Paragraph(new Run(viewModel.EditorPlainText)));
        }
        finally { isLoadingEditor = false; }
    }

    private void DocumentEditor_TextChanged(object sender, TextChangedEventArgs e) => CommitEditor();

    private void CommitEditor()
    {
        if (isLoadingEditor || viewModel is null) return;
        var range = new TextRange(DocumentEditor.Document.ContentStart, DocumentEditor.Document.ContentEnd);
        using var stream = new MemoryStream();
        range.Save(stream, DataFormats.Rtf);
        viewModel.SetEditorContent(System.Text.Encoding.UTF8.GetString(stream.ToArray()), range.Text.TrimEnd('\r', '\n'));
    }

    private void BoldButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        var weight = DocumentEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
        DocumentEditor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, weight is FontWeight value && value == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
        CommitEditor();
    }

    private void ItalicButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        var style = DocumentEditor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
        DocumentEditor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, style is FontStyle value && value == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
        CommitEditor();
    }

    private void UnderlineButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        EditingCommands.ToggleUnderline.Execute(null, DocumentEditor);
        CommitEditor();
    }

    private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        EditingCommands.AlignLeft.Execute(null, DocumentEditor);
        CommitEditor();
    }

    private void CenterButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        if (DocumentEditor.Selection.Start.Paragraph?.TextAlignment == TextAlignment.Center) EditingCommands.AlignLeft.Execute(null, DocumentEditor);
        else EditingCommands.AlignCenter.Execute(null, DocumentEditor);
        CommitEditor();
    }

    private void AlignRightButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        EditingCommands.AlignRight.Execute(null, DocumentEditor);
        CommitEditor();
    }

    private void DecreaseFontButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyFontSizeDelta(-1d);
    }

    private void IncreaseFontButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyFontSizeDelta(1d);
    }

    private void ApplyFontSizeDelta(double delta)
    {
        FocusEditor();
        var size = DocumentEditor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
        var current = size is double value ? value : 15d;
        DocumentEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, Math.Clamp(current + delta, 9d, 28d));
        CommitEditor();
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        if (DocumentEditor.CanUndo) DocumentEditor.Undo();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        FocusEditor();
        if (DocumentEditor.CanRedo) DocumentEditor.Redo();
    }

    private void FocusEditor() => DocumentEditor.Focus();
}

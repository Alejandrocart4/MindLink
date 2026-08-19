using System.Windows;
using System.Windows.Controls;
using MindLink.Presentation.ViewModels;

namespace MindLink.Presentation.Views;

public partial class NotesView : UserControl
{
    public NotesView()
    {
        InitializeComponent();
    }

    private void NotesList_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not ListBox list || e.OriginalSource is not DependencyObject source) return;
        if (ItemsControl.ContainerFromElement(list, source) is ListBoxItem) return;
        if (DataContext is NotesViewModel viewModel) viewModel.ClearSelectedNote();
    }

    private void StatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.ContextMenu is null) return;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
        e.Handled = true;
    }

    private void StatusMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem item || item.Parent is not ContextMenu menu ||
            menu.PlacementTarget is not Button { Tag: MindLink.Application.Models.WorkspaceNote note } ||
            DataContext is not NotesViewModel viewModel) return;
        viewModel.ChangeNoteStatus(note, item.Tag as string ?? string.Empty);
    }
}

using Microsoft.Win32;

namespace MindLink.Presentation.Services;

public sealed class FileDialogService : IFileDialogService
{
    public string? SelectProjectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir proyecto de MindLink",
            Filter = "Proyecto MindLink (*.mindlink)|*.mindlink|Documentos de investigación (*.md;*.docx;*.txt)|*.md;*.docx;*.txt|Todos los archivos (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

using System.Windows;
using System.Windows.Controls;

namespace MindLink.Presentation.Controls;

public partial class MindLinkBrand : UserControl
{
    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(MindLinkBrand), new PropertyMetadata(32d));

    public MindLinkBrand() => InitializeComponent();

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

}

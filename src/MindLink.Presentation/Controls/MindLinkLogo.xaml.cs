using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindLink.Presentation.Controls;

public partial class MindLinkLogo : UserControl
{
    public static readonly DependencyProperty LogoSizeProperty = DependencyProperty.Register(
        nameof(LogoSize), typeof(double), typeof(MindLinkLogo), new PropertyMetadata(32d));

    public static readonly DependencyProperty TextFontSizeProperty = DependencyProperty.Register(
        nameof(TextFontSize), typeof(double), typeof(MindLinkLogo), new PropertyMetadata(17d));

    public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
        nameof(ShowText), typeof(bool), typeof(MindLinkLogo), new PropertyMetadata(true));

    public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
        nameof(TextForeground), typeof(Brush), typeof(MindLinkLogo), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 50, 128))));

    public MindLinkLogo() => InitializeComponent();

    public double LogoSize
    {
        get => (double)GetValue(LogoSizeProperty);
        set => SetValue(LogoSizeProperty, value);
    }

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public bool ShowText
    {
        get => (bool)GetValue(ShowTextProperty);
        set => SetValue(ShowTextProperty, value);
    }

    public Brush TextForeground
    {
        get => (Brush)GetValue(TextForegroundProperty);
        set => SetValue(TextForegroundProperty, value);
    }
}

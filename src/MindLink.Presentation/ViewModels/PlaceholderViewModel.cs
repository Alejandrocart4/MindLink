using MindLink.Presentation.Navigation;
namespace MindLink.Presentation.ViewModels;
public sealed class PlaceholderViewModel:ObservableObject {public string ModuleName{get;}public RelayCommand BackCommand{get;}public PlaceholderViewModel(NavigationService navigation,string moduleName){ModuleName=moduleName;BackCommand=new(navigation.NavigateToDashboard);}}

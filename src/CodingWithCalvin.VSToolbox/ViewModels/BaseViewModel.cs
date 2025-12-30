namespace CodingWithCalvin.VSToolbox.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; }
}

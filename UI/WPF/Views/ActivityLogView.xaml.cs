using System.Windows.Controls;
using WPF.ViewModels;

namespace WPF.Views;

public partial class ActivityLogView : UserControl
{
    public ActivityLogView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ActivityLogViewModel vm)
        {
            await vm.LoadUsersCommand.ExecuteAsync(null);
            await vm.LoadLogsCommand.ExecuteAsync(null);
        }
    }
}

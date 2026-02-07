using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WPF.Messages;

public class NavigateToViewMessage : ValueChangedMessage<string>
{
    public NavigateToViewMessage(string viewModelName) : base(viewModelName) { }
}

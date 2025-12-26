using WebUI.Shared.Enums;

namespace WebUI.Shared.UIControls;

public interface INotifier
{
    void Show(string message, ToastType type);
}

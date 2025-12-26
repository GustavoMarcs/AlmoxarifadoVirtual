using Blazored.Toast.Services;
using WebUI.Shared.Enums;

namespace WebUI.Shared.UIControls;

public class Notifier(IToastService toast) : INotifier
{
    public void Show(string message, ToastType type)
    {
        if (toast is null)
            throw new InvalidOperationException("ToastService not configurated. Make sure to inject IToastHelper");

        switch (type)
        {
            case ToastType.Success:
                toast.ShowSuccess(message);
                break;
            case ToastType.Error:
                toast.ShowError(message);
                break;
            case ToastType.Info:
                toast.ShowInfo(message);
                break;
            case ToastType.Warning:
                toast.ShowWarning(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
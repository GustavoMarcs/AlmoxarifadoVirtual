using Microsoft.AspNetCore.Components;

namespace WebUI;

public abstract class ApplicationComponentBase : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _debounceCancellationTokenSource;


    protected CancellationToken CancellationToken => (_cancellationTokenSource
        ??= new CancellationTokenSource()).Token;

    public void Dispose()
    {
        if (_debounceCancellationTokenSource is not null)
        {
            _debounceCancellationTokenSource.Cancel();
            _debounceCancellationTokenSource.Dispose();
            _debounceCancellationTokenSource = null;
        }

        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        GC.SuppressFinalize(this);
    }

    protected async Task Debounce(Func<Task> action, int delay = 300)
    {
        _debounceCancellationTokenSource?.Cancel();
        _debounceCancellationTokenSource?.Dispose();
        _debounceCancellationTokenSource = new CancellationTokenSource();
        var localCts = _debounceCancellationTokenSource;

        try
        {
            await Task.Delay(delay, localCts.Token);
            if (!localCts.IsCancellationRequested && !CancellationToken.IsCancellationRequested)
            {
                await action();
            }
        }
        catch (OperationCanceledException)
        {
            // swallow
        }
    }
}

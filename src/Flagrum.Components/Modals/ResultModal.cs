namespace Flagrum.Components.Modals;

public class ResultModal<TResult> : AutosizeModal
{
    private TaskCompletionSource<TResult>? _taskCompletionSource;

    public override void Close(Action? onClose = null)
    {
        _taskCompletionSource?.SetCanceled();
        _taskCompletionSource = null;
        base.Close(onClose);
    }

    public void SetResult(TResult result)
    {
        _taskCompletionSource?.SetResult(result);
        _taskCompletionSource = null;
    }

    public async Task<(bool Success, TResult? Result)> TryGetResultAsync()
    {
        try
        {
            return (true, await GetResult());
        }
        catch (TaskCanceledException)
        {
            return (false, default);
        }
    }

    private async Task<TResult> GetResult()
    {
        _taskCompletionSource = new TaskCompletionSource<TResult>();

        Display = "flex";
        await InvokeAsync(StateHasChanged);

        await Task.Delay(50);

        Opacity = "opacity-100";
        await InvokeAsync(StateHasChanged);

        return await _taskCompletionSource.Task;
    }
}
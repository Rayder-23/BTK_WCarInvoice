using CommunityToolkit.Mvvm.ComponentModel;

namespace CarInvoice.Desktop.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string? _statusMessage;
    private StatusSeverity _statusSeverity = StatusSeverity.Neutral;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
                OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public StatusSeverity StatusSeverity
    {
        get => _statusSeverity;
        set => SetProperty(ref _statusSeverity, value);
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    /// <summary>Sets status text and semantic colour in one call.</summary>
    protected void SetStatus(string? message, StatusSeverity severity = StatusSeverity.Neutral)
    {
        StatusSeverity = severity;
        StatusMessage = message;
    }
}

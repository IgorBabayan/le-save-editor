namespace LastEpochSaveEditor.ViewModels.Dialog;

public interface IViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    bool? Result { get; }
}

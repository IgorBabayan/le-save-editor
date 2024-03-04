namespace LastEpochSaveEditor.ViewModels;

public interface IViewModel : INotifyPropertyChanged, INotifyPropertyChanging 
{
	bool? Result { get; }
}

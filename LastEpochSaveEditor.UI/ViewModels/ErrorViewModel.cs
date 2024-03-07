namespace LastEpochSaveEditor.ViewModels;

internal interface IErrorViewModel : IViewModel
{
	void SetData(string title, string message);
}

internal partial class ErrorViewModel : ObservableObject, IErrorViewModel
{
	private readonly IErrorView _errorView;
	public bool? Result { get; private set; }

	#region Properties

	[ObservableProperty]
	private string? _title;

	[ObservableProperty]
	private string? _message;

	#endregion

	#region IErrorViewModel

	public void SetData(string title, string message)
	{
		Title = title;
		Message = message;
	}

	#endregion

	#region Commands

	[RelayCommand]
	private void Close(object param)
	{
		if (param == null)
			Result = null;

		if (param is string arg)
			Result = bool.Parse(arg);

		_errorView.CloseDialog();
	}

	#endregion

	public ErrorViewModel(IErrorView errorView) => _errorView = errorView;
}
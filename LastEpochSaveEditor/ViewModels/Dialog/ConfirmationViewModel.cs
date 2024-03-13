namespace LastEpochSaveEditor.ViewModels.Dialog;

internal interface IConfirmationViewModel : IViewModel
{
	void SetData(string title, string message);
}

internal partial class ConfirmationViewModel : ObservableObject, IConfirmationViewModel
{
	private readonly IConfirmationView _confirmationView;

	public bool? Result { get; private set; }

	#region Properties

	[ObservableProperty]
	private string? _title;

	[ObservableProperty]
	private string? _message;

	#endregion

	#region IConfirmationViewModel

	public void SetData(string title, string message)
	{
		Title = title;
		Message = message;
	}

	#endregion

	#region Commands

	[RelayCommand]
	private async Task Close(object param)
	{
		if (param == null)
			Result = null;

		if (param is string arg)
			Result = bool.Parse(arg);

		await _confirmationView.CloseDialog();
	}

	#endregion

	public ConfirmationViewModel(IConfirmationView confirmationView) => _confirmationView = confirmationView;
}

namespace LastEpochSaveEditor.ViewModels;

internal interface IMessageViewModel : IViewModel
{
	void SetData(string title, string message);
}

internal partial class MessageViewModel : ObservableObject, IMessageViewModel
{
	private readonly IMessageView _messageView;
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

		await _messageView.CloseDialog();
	}

	#endregion

	public MessageViewModel(IMessageView messageView) => _messageView = messageView;
}

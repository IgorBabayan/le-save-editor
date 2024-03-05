namespace LastEpochSaveEditor.Services.Dialog;

internal interface IDialogService
{
	Task ShowMessage(string message);
	Task<bool?> ShowConfirmation(string message);
	Task<bool?> ShowError(string message);
	Task ShowCustomDialog<TView, TModel>(TModel viewModel)
		where TView : class, IView
		where TModel : IViewModel;
}

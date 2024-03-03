namespace LastEpochSaveEditor.Services;

internal interface IDialogService
{
	Task ShowMessage(string message, string title);
	Task<bool> ShowConfirmation(string message, string title);
	Task ShowCustomDialog<TView>(object viewModel)
		where TView : class, IView;
}

internal class DialogService : IDialogService
{
	public async Task ShowMessage(string message, string title)
	{
		/*var dialog = new ContentDialog
		{
			Title = title,
			Content = message,
			CloseButtonText = "OK"
		};

		await dialog.ShowAsync();*/
	}

	public async Task<bool> ShowConfirmation(string message, string title)
	{
		/*var dialog = new ContentDialog
		{
			Title = title,
			Content = message,
			PrimaryButtonText = "Yes",
			CloseButtonText = "No"
		};

		var result = await dialog.ShowAsync();
		return result == ContentDialogResult.Primary;*/

		return default;
	}

	public async Task ShowCustomDialog<TView>(object viewModel)
		where TView : class, IView
	{
		var view = App.GetService<TView>();
		view.DataContext = viewModel;

		await view.ShowDialog();
	}
}

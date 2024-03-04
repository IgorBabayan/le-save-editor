namespace LastEpochSaveEditor.Services;

internal interface IDialogService
{
	Task ShowMessage(string message);
	Task<bool?> ShowConfirmation(string message);
	Task<bool?> ShowError(string message);
	Task ShowCustomDialog<TView, TModel>(TModel viewModel)
		where TView : class, IView
		where TModel : IViewModel;
}

internal class DialogService : IDialogService
{
	public async Task ShowMessage(string message)
	{
		var viewModel = App.GetService<IMessageViewModel>();
		viewModel.SetData(APPLICATION_TITLE, message);

		var window = CreateDialogWindow<IMessageView, IMessageViewModel>(viewModel);
		await window.ShowDialog();
	}

	public async Task<bool?> ShowError(string message)
	{
		var viewModel = App.GetService<IErrorViewModel>();
		viewModel.SetData(APPLICATION_TITLE, message);

		var window = CreateDialogWindow<IErrorView, IErrorViewModel>(viewModel);
		await window.ShowDialog();
		return viewModel.Result;
	}

	public async Task<bool?> ShowConfirmation(string message)
	{
		var viewModel = App.GetService<IConfirmationViewModel>();
		viewModel.SetData(APPLICATION_TITLE, message);

		var window = CreateDialogWindow<IConfirmationView, IConfirmationViewModel>(viewModel);
		await window.ShowDialog();
		return viewModel.Result;
	}

	public async Task ShowCustomDialog<TView, TModel>(TModel viewModel)
		where TView : class, IView
		where TModel : IViewModel
	{
		var window = CreateDialogWindow<TView, TModel>(viewModel);
		await window.ShowDialog();
	}

	private TView CreateDialogWindow<TView, TModel>(TModel viewModel)
		where TView : class, IView
		where TModel : IViewModel
	{
		var window = App.GetService<TView>();
		window.Owner = Application.Current.MainWindow;
		window.Width = Application.Current.MainWindow.Width;
		window.Height = Application.Current.MainWindow.Height;
		window.DataContext = viewModel;

		return window;
	}
}

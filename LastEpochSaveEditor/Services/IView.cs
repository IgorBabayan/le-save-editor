namespace LastEpochSaveEditor.Services;

internal interface IView
{
	double Width { get; set; }
	double Height { get; set; }
	Window Owner { get; set; }
	Task ShowDialog();
	Task CloseDialog();
	object DataContext { get; set; }
}

internal interface IDownloadView : IView { }
internal interface IErrorView : IView { }
internal interface IConfirmationView : IView { }
internal interface IMessageView : IView { }
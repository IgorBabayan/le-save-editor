namespace LastEpochSaveEditor.Services;

internal interface IView
{
	Task ShowDialog();
	object DataContext { get; set; }
}

internal interface IDownloadView : IView { }
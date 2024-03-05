namespace LastEpochSaveEditor.Services.Dialog;

internal interface IView
{
	double Width { get; set; }
	double Height { get; set; }
	Task ShowDialog();
	Task CloseDialog();
	object DataContext { get; set; }
}

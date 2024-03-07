namespace LastEpochSaveEditor.Popups;

public partial class MessageWindow : Window, IMessageView
{
	public MessageWindow() => InitializeComponent();

	void IView.CloseDialog() => Hide();

	async Task IView.ShowDialog() => throw new NotImplementedException();

	/*async Task IView.CloseDialog() => await Application.Current.Dispatcher.InvokeAsync(() => Hide());

	async Task IView.ShowDialog() => await Application.Current.Dispatcher.InvokeAsync(() => ShowDialog());*/
}
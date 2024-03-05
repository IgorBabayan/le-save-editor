namespace LastEpochSaveEditor.Views;

public partial class MainWindow : Window
{
	private bool _mouseDownForWindowMoving = false;
	private PointerPoint _originalPoint;
	
	public MainWindow() => InitializeComponent();

	private void OnPointerPressed(object? sender, PointerPressedEventArgs args)
	{
		_originalPoint = args.GetCurrentPoint(this);
		_mouseDownForWindowMoving = true;
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => _mouseDownForWindowMoving = false;

	private void OnPointerMoved(object? sender, PointerEventArgs args)
	{
		if (!_mouseDownForWindowMoving)
			return;

		var currentPoint = args.GetCurrentPoint(this);
		Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
			Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
	}

	private void CloseOnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
			desktopApp.Shutdown();
	}
}
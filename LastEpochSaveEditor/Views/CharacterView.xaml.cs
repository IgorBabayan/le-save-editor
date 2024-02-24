namespace LastEpochSaveEditor.Views;

public partial class CharacterView
{
	public CharacterView()
	{
		InitializeComponent();
		DataContext = App.GetService<CharacterViewModel>();
	}
}
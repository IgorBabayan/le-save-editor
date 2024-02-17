using LastEpochSaveEditor.ViewModels;
using System.Windows.Controls;

namespace LastEpochSaveEditor.Controls
{
	public partial class CharacterControl : UserControl
	{
		public CharacterControl()
		{
			InitializeComponent();
			DataContext = App.GetService<CharacterViewModel>();
		}
	}
}

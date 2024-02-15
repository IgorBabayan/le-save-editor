using LastEpochSaveEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace LastEpochSaveEditor.Controls
{
	public partial class CharacterControl : UserControl
	{
		public CharacterControl()
		{
			InitializeComponent();
			DataContext = App.Host!.Services.GetRequiredService<CharacterViewModel>();
		}
	}
}

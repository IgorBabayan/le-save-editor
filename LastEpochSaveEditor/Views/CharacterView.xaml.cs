﻿namespace LastEpochSaveEditor.Views
{
	public partial class CharacterView : UserControl
	{
		public CharacterView()
		{
			InitializeComponent();
			DataContext = App.GetService<CharacterViewModel>();
		}
	}
}
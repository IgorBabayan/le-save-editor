using System;
using System.Windows;

namespace LastEpochSaveEditor.Utils
{
	internal class AppTheme
    {
        public static void ChangeTheme(bool isSoft)
        {
            var theme = new ResourceDictionary
            {
                Source =  isSoft
                    ? new Uri("Themes/AuraSoft.xaml", UriKind.Relative)
                    : new Uri("Themes/Aura.xaml", UriKind.Relative)
            };

            App.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(theme);
        }
    }
}

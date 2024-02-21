namespace LastEpochSaveEditor.Controls
{
	public partial class ItemButton : UserControl
    {
        public ItemButton()
        {
            InitializeComponent();
        }


        public string HelmIcon
		{
            get { return (string)GetValue(HelmIconProperty); }
            set { SetValue(HelmIconProperty, value); }
        }

        public static readonly DependencyProperty HelmIconProperty =
            DependencyProperty.Register("HelmIcon", typeof(string), typeof(ItemButton), new PropertyMetadata(null));


    }
}

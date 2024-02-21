namespace LastEpochSaveEditor.Controls
{
	public partial class ItemImage : UserControl
    {
		public ItemImage() => InitializeComponent();

		public QualityType Quality
		{
			get { return (QualityType)GetValue(QualityProperty); }
			set { SetValue(QualityProperty, value); }
		}

		public static readonly DependencyProperty QualityProperty =
			DependencyProperty.Register("Quality", typeof(QualityType), typeof(ItemImage), new PropertyMetadata(null));

		public int IconWidth
		{
			get { return (int)GetValue(IconWidthProperty); }
			set { SetValue(IconWidthProperty, value); }
		}

		public static readonly DependencyProperty IconWidthProperty =
			DependencyProperty.Register("IconWidth", typeof(int), typeof(ItemImage), new PropertyMetadata(0, new(OnIconWidthChanged)));
		private static void OnIconWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			var element = sender as ItemImage;
			if (element != null)
				element.Width = (int)args.NewValue;
		}

		public int IconHeight
		{
			get { return (int)GetValue(IconHeightProperty); }
			set { SetValue(IconHeightProperty, value); }
		}

		public static readonly DependencyProperty IconHeightProperty =
			DependencyProperty.Register("IconHeight", typeof(int), typeof(ItemImage), new PropertyMetadata(0, new(OnIconHeightChanged)));

		private static void OnIconHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			var element = sender as ItemImage;
			if (element != null)
				element.Height = (int)args.NewValue;
		}

		public Thickness IconMargin
		{
			get { return (Thickness)GetValue(IconMarginProperty); }
			set { SetValue(IconMarginProperty, value); }
		}

		public static readonly DependencyProperty IconMarginProperty =
			DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(ItemImage), new PropertyMetadata(null));

		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(ImageSource), typeof(ItemImage), new PropertyMetadata(null));
	}
}

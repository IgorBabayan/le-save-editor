namespace LastEpochSaveEditor.ViewModels;

internal interface IDownloadViewModel : IViewModel { }

internal partial class DownloadViewModel : ObservableObject, IDownloadViewModel
{
	private readonly ILogger<DownloadViewModel> _logger;
	private readonly IDatabaseService _db;
	private readonly IDownloadView _downloadView;
	private readonly IDialogService _dialogService;
	private readonly ISubItemRepositoryFactory _subItemRepositoryFactory;
	private readonly IUniqueRepositoryFactory _uniqueRepositoryFactory;
	private readonly ISetRepositoryFactory _setRepositoryFactory;
	private bool _hasError;

	public bool? Result { get; private set; }

	#region Properties

	[ObservableProperty]
	private int _downloadProgress;

	[ObservableProperty]
	private int _convertProgress;

	[ObservableProperty]
	private int _removeProgress;

	[ObservableProperty]
	private string _downloadProgressText;

	[ObservableProperty]
	private string _convertProgressText;

	[ObservableProperty]
	private string _removeProgressText;

	[ObservableProperty]
	private int _count;

	[ObservableProperty]
	private bool _canDownload;

	#endregion

	#region Commands

	[RelayCommand]
	private async Task Download()
	{
		CanDownload = false;
		await DownloadImages();
	}

	[RelayCommand]
	private async Task Close() => await _downloadView.CloseDialog();

	#endregion

	public DownloadViewModel(ILogger<DownloadViewModel> logger, IDatabaseService db, IDownloadView downloadView, IDialogService dialogService,
		ISubItemRepositoryFactory subItemRepositoryFactory, IUniqueRepositoryFactory uniqueRepositoryFactory, ISetRepositoryFactory setRepositoryFactory)
	{
		_logger = logger;
		_db = db;
		_downloadView = downloadView;
		_dialogService = dialogService;
		_subItemRepositoryFactory = subItemRepositoryFactory;
		_uniqueRepositoryFactory = uniqueRepositoryFactory;
		_setRepositoryFactory = setRepositoryFactory;
		
		Count = int.MaxValue;
		CanDownload = true;
	}

	private async Task<IDictionary<string, List<string>>> PrepareFilesAndFolders()
	{
		if (Directory.Exists(IMAGE_FOLDER_NAME))
			Directory.Delete(IMAGE_FOLDER_NAME, true);
		Directory.CreateDirectory(IMAGE_FOLDER_NAME);


		var result = new Dictionary<string, List<string>>(Count);
		var basicRepository = await _subItemRepositoryFactory.Create();
		var uniqueRepository = await _uniqueRepositoryFactory.Create();
		var setRepository = await _setRepositoryFactory.Create();

		string rootPath;
		IDictionary<ItemInfoTypeEnum, IEnumerable<SubItem>> basicItems;
		IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> uniqueItems;
		IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> setItems;
		foreach (var folders in FolderStructure.Folders)
        {
			rootPath = Path.Combine(IMAGE_FOLDER_NAME, folders.Key.GetDescriptionName());
			Directory.CreateDirectory(rootPath);

			basicItems = basicRepository.GetByType(folders.Key);
			uniqueItems = uniqueRepository.GetByType(folders.Key);
			setItems = setRepository.GetByType(folders.Key);
			PrepareBasicItems(BASIC_FOLDER_NAME, rootPath, result, basicItems);
			PrepareUniqueOrSetItems(UNIQUE_FOLDER_NAME, rootPath, result, uniqueItems);
			PrepareUniqueOrSetItems(SET_FOLDER_NAME, rootPath, result, setItems);
		}

		return result;
    }

	private void PrepareBasicItems(string quality, string rootPath, IDictionary<string, List<string>> result,
		IDictionary<ItemInfoTypeEnum, IEnumerable<SubItem>> items)
	{
		string folderPath;
		string path;
		string key;
        foreach (var item in items)
        {
			key = item.Key.GetDescriptionName();
			folderPath = Path.Combine(rootPath, key);
			Directory.CreateDirectory(folderPath);

			path = Path.Combine(folderPath, quality);
			Directory.CreateDirectory(path);

			if (result.ContainsKey(key))
				result[key].AddRange(item.Value.Select(x => Path.Combine(path, x.Name.GetItemNameAsWebP())));
			else
				result[key] = new List<string>(item.Value.Select(x => Path.Combine(path, x.Name.GetItemNameAsWebP())));
		}
    }

	private void PrepareUniqueOrSetItems(string quality, string rootPath, Dictionary<string, List<string>> result, 
		IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> items)
	{
		string folderPath;
		string path;
		string key;
		foreach (var item in items)
		{
			key = item.Key.GetDescriptionName();
			folderPath = Path.Combine(rootPath, key);
			Directory.CreateDirectory(folderPath);

			path = Path.Combine(folderPath, quality);
			Directory.CreateDirectory(path);

			if (result.ContainsKey(key))
				result[key].AddRange(item.Value.Select(x => Path.Combine(path, x.Name.GetItemNameAsWebP())));
			else
				result[key] = new List<string>(item.Value.Select(x => Path.Combine(path, x.Name.GetItemNameAsWebP())));
		}
	}

	private async Task DownloadImages()
	{
		Count = await _db.Count();

		DownloadProgress = 0;
		ConvertProgress = 0;
		RemoveProgress = 0;

		var files = await PrepareFilesAndFolders();
		foreach (var file in files)
		{
            foreach (var path in file.Value)
            {
				await DownloadFile(file.Key, path);
				//await ProcessFile(UNIQUE_FOLDER_NAME, name);
				//await DeleteFile(UNIQUE_FOLDER_NAME, name);
			}
		}

		CanDownload = true;

		if (_hasError)
			await _dialogService.ShowError($"Download images was finished, but there was error(s).{Environment.NewLine}You can check log file: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")}");
		else
			await _dialogService.ShowMessage("Download images finished successfully!");
	}

	private async Task DownloadFile(string baseType, string path)
	{
		Application.Current.Dispatcher.Invoke(() => DownloadProgressText = Path.GetFileName(path));
		_logger.LogInformation($"Download: {DownloadProgressText}");

		try
		{
			await Task.Delay(TimeSpan.FromMilliseconds(100));
			//await FileDownloader.DownloadImage(baseType, path);
		}
		catch (Exception exception)
		{
			_hasError = true;
			_logger.LogError($"Failed to download: {DownloadProgressText}", exception);
		}
		finally
		{
			Application.Current.Dispatcher.Invoke(() => DownloadProgress++);
		}
	}

	private async Task ConvertFile(string baseType, string name)
	{
		try
		{
			ConvertProgressText = Path.GetFileName(name);
			_logger.LogInformation($"Convert: {ConvertProgressText}");

			//await WebPConverter.Convert(name);
			ConvertProgress++;
		}
		catch (Exception exception)
		{
			_hasError = true;
			_logger.LogError($"Failed to convert: {ConvertProgressText}", exception);
		}
	}

	private async Task DeleteFile(string baseType, string name)
	{
		await Task.Run(() =>
		{
			try
			{
				RemoveProgressText = Path.GetFileName(name);
				_logger.LogInformation($"Remove: {RemoveProgressText}");

				//File.Delete(file.Path);
				RemoveProgress++;
			}
			catch (Exception exception)
			{
				_hasError = true;
				_logger.LogError($"Failed to remove: {RemoveProgressText}", exception);
			}
		});
	}
}
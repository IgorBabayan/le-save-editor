namespace LastEpochSaveEditor.ViewModels.Dialog;

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
	private readonly IAffixRepositoryFactory _affixRepositoryFactory;
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
		ISubItemRepositoryFactory subItemRepositoryFactory, IUniqueRepositoryFactory uniqueRepositoryFactory, ISetRepositoryFactory setRepositoryFactory,
		IAffixRepositoryFactory affixRepositoryFactory)
	{
		_logger = logger;
		_db = db;
		_downloadView = downloadView;
		_dialogService = dialogService;
		_subItemRepositoryFactory = subItemRepositoryFactory;
		_uniqueRepositoryFactory = uniqueRepositoryFactory;
		_setRepositoryFactory = setRepositoryFactory;
		_affixRepositoryFactory = affixRepositoryFactory;
		
		Count = int.MaxValue;
		CanDownload = true;
	}

	private async Task<IDictionary<string, List<string>>> PrepareFilesAndFolders()
	{
		if (Directory.Exists(IMAGE_FOLDER_NAME))
			Directory.Delete(IMAGE_FOLDER_NAME, true);
		Directory.CreateDirectory(IMAGE_FOLDER_NAME);


		var result = new Dictionary<string, List<string>>();
		var basicRepository = await _subItemRepositoryFactory.Create();
		var uniqueRepository = await _uniqueRepositoryFactory.Create();
		var setRepository = await _setRepositoryFactory.Create();
		var affixRepository = await _affixRepositoryFactory.Create();

		string rootPath;
		IDictionary<ItemInfoTypeEnum, IEnumerable<SubItem>> basicItems;
		IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> uniqueItems;
		IDictionary<ItemInfoTypeEnum, IEnumerable<Unique>> setItems;
		IEnumerable<Affix> affixItems;
		foreach (var folders in FolderStructure.Folders)
        {
			rootPath = Path.Combine(IMAGE_FOLDER_NAME, folders.Key.GetDescriptionName());
			Directory.CreateDirectory(rootPath);

			switch (folders.Key)
			{
				case ItemInfoTypeEnum.Lens:
				case ItemInfoTypeEnum.Misc:
					basicItems = basicRepository.GetByType(folders.Key);
					PrepareMiscItems(rootPath, result, basicItems);
					break;

				case ItemInfoTypeEnum.AffixShard:
					affixItems = affixRepository.GetAll();
					PrepareAffixItems(rootPath, result, affixItems);
					break;

				default:
					basicItems = basicRepository.GetByType(folders.Key);
					uniqueItems = uniqueRepository.GetByType(folders.Key);
					setItems = setRepository.GetByType(folders.Key);

					PrepareBasicItems(BASIC_FOLDER_NAME, rootPath, result, basicItems);
					PrepareUniqueOrSetItems(UNIQUE_FOLDER_NAME, rootPath, result, uniqueItems);
					PrepareUniqueOrSetItems(SET_FOLDER_NAME, rootPath, result, setItems);
					break;
			}
        }

		return result;
    }

	private void PrepareAffixItems(string rootPath, IDictionary<string, List<string>> result, IEnumerable<Affix> items)
	{
		string key = ItemInfoTypeEnum.AffixShard.GetDescriptionName();
		if (result.ContainsKey(key))
			result[key].AddRange(items.Select(x => Path.Combine(rootPath, $"{SHARD_PREFIX}{x.Group}.webp")));
		else
			result[key] = new List<string>(items.Select(x => Path.Combine(rootPath, $"{SHARD_PREFIX}{x.Group}.webp")));

		result[key] = result[key].Distinct().ToList();
	}

	private void PrepareMiscItems(string rootPath, IDictionary<string, List<string>> result, IDictionary<ItemInfoTypeEnum, IEnumerable<SubItem>> items)
	{
		string folderPath;
		string key;
		foreach (var item in items)
		{
			key = item.Key.GetDescriptionName();
			folderPath = Path.Combine(rootPath, key);
			Directory.CreateDirectory(folderPath);

			if (result.ContainsKey(key))
				result[key].AddRange(item.Value.Select(x => Path.Combine(folderPath, x.Name.GetItemNameAsWebP())));
			else
				result[key] = new List<string>(item.Value.Select(x => Path.Combine(folderPath, x.Name.GetItemNameAsWebP())));
		}
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
		DownloadProgress = 0;
		ConvertProgress = 0;
		RemoveProgress = 0;

		var files = await PrepareFilesAndFolders();
		Count = files.Sum(x => x.Value.Count);
		foreach (var file in files)
		{
            foreach (var path in file.Value)
            {
				await DownloadFile(file.Key, path);
				await ConvertFile(path);
				await DeleteFile(path);
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
			await FileDownloader.DownloadImage(baseType, path);
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

	private async Task ConvertFile(string path)
	{
		Application.Current.Dispatcher.Invoke(() => ConvertProgressText = Path.GetFileName(path));
		_logger.LogInformation($"Convert: {ConvertProgressText}");

		try
		{
			//await Task.Delay(TimeSpan.FromMilliseconds(50));
			await WebPConverter.Convert(path);
		}
		catch (Exception exception)
		{
			_hasError = true;
			_logger.LogError($"Failed to convert: {ConvertProgressText}", exception);
		}
		finally
		{
			Application.Current.Dispatcher.Invoke(() => ConvertProgress++);
		}
	}

	private async Task DeleteFile(string path)
	{
		Application.Current.Dispatcher.Invoke(() => RemoveProgressText = Path.GetFileName(path));
		_logger.LogInformation($"Remove: {RemoveProgressText}");
		
		try
		{
			await Task.Run(() => File.Delete(path));
		}
		catch (Exception exception)
		{
			_hasError = true;
			_logger.LogError($"Failed to remove: {RemoveProgressText}", exception);
		}
		finally
		{
			Application.Current.Dispatcher.Invoke(() => RemoveProgress++);
		}
	}
}
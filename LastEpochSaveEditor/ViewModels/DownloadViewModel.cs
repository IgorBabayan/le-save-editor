namespace LastEpochSaveEditor.ViewModels;

internal interface IDownloadViewModel : IViewModel { }

internal partial class DownloadViewModel : ObservableObject, IDownloadViewModel
{
	private readonly ILogger<DownloadViewModel> _logger;
	private readonly IDatabaseService _db;
	private readonly IDownloadView _downloadView;
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
	
		PrepareFolders();
		await DownloadImages();
	}

	[RelayCommand]
	private async Task Close() => await _downloadView.CloseDialog();

	#endregion

	public DownloadViewModel(ILogger<DownloadViewModel> logger, IDatabaseService db, IDownloadView downloadView)
	{
		_logger = logger;
		_db = db;
		_downloadView = downloadView;
		
		Count = int.MaxValue;
		CanDownload = true;
	}

	private void PrepareFolders()
	{
		return;
		/*if (Directory.Exists(Const.IMAGE_FOLDER_NAME))
			Directory.Delete(Const.IMAGE_FOLDER_NAME, true);
		Directory.CreateDirectory(Const.IMAGE_FOLDER_NAME);

		string rootPath;
		string path;
		string folderPath;
		foreach (var item in _db.GetFolderStructure())
		{
			rootPath = Path.Combine(Const.IMAGE_FOLDER_NAME, item.Key);
			if (!Directory.Exists(rootPath))
				Directory.CreateDirectory(rootPath);

			foreach (var type in item.Value)
			{
				var name = type.Base.BaseTypeName;
				path = Path.Combine(rootPath, name);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				folderPath = Path.Combine(path, Const.BASIC_FOLDER_NAME);
				Directory.CreateDirectory(folderPath);

				folderPath = Path.Combine(path, Const.UNIQUE_FOLDER_NAME);
				Directory.CreateDirectory(folderPath);

				folderPath = Path.Combine(path, Const.SET_FOLDER_NAME);
				Directory.CreateDirectory(folderPath);
			}
		}*/
	}

	private async Task DownloadImages()
	{
		var window = App.GetService<IDialogService>();
		await window.ShowConfirmation("Hello world");

		Count = await _db.Count();
		return;
		DownloadProgress = 0;
		ConvertProgress = 0;
		RemoveProgress = 0;

		var files = GetFilesForDownload();
		var worker = new BackgroundWorker();
		worker.WorkerReportsProgress = true;
		worker.DoWork += async (s, e) =>
		{
			foreach (var file in files)
			{
				await DownloadFile(file);
				await ProcessFile(file);
				await DeleteFile(file);
			}

			CanDownload = true;
		};

		worker.RunWorkerAsync();
	}

	private IEnumerable<FolderStructure> GetFilesForDownload()
	{
		throw new NotImplementedException ();
		/*string rootPath;
		string path;
		string folderPath;
		string fileName;
		var result = new List<FolderStructure>(Count);
		foreach (var item in _db.GetFolderStructure())
		{
			rootPath = Path.Combine(Const.IMAGE_FOLDER_NAME, item.Key);
			foreach (var type in item.Value)
			{
				var name = type.Base.BaseTypeName;
				path = Path.Combine(rootPath, name);

				folderPath = Path.Combine(path, Const.BASIC_FOLDER_NAME);
				foreach (var baseItem in type.Base.SubItems)
				{
					fileName = baseItem.Name.ToLowerInvariant().Replace(" ", "_");
					result.Add(new(name.ToLowerInvariant().Replace(" ", "_"), $"{Path.Combine(folderPath, fileName)}.webp"));
				}

				folderPath = Path.Combine(path, Const.UNIQUE_FOLDER_NAME);
				foreach (var unique in type.Uniques)
				{
					fileName = unique.Name.ToLowerInvariant().Replace(" ", "_");
					result.Add(new(Const.UNIQUE_FOLDER_NAME.ToLowerInvariant().Replace(" ", "_"), $"{Path.Combine(folderPath, fileName)}.webp"));
				}

				folderPath = Path.Combine(path, Const.SET_FOLDER_NAME);
				foreach (var set in type.Sets)
				{
					fileName = set.Name.ToLowerInvariant().Replace(" ", "_");
					result.Add(new(Const.UNIQUE_FOLDER_NAME.ToLowerInvariant().Replace(" ", "_"), $"{Path.Combine(folderPath, fileName)}.webp"));
				}
			}
		}
			
		return result;*/
	}

	private async Task DownloadFile(FolderStructure file)
	{
		try
		{
			DownloadProgressText = Path.GetFileName(file.Path);
			_logger.LogInformation($"Download: {DownloadProgressText}");

			await FileDownloader.DownloadImage(file.Type, file.Path);
			DownloadProgress++;
		}
		catch (Exception exception)
		{
			_logger.LogError($"Failed to download: {DownloadProgressText}", exception);
		}
	}

	private async Task ProcessFile(FolderStructure file)
	{
		try
		{
			ConvertProgressText = Path.GetFileName(file.Path);
			_logger.LogInformation($"Convert: {ConvertProgressText}");

			await WebPConverter.Convert(file.Path);
			ConvertProgress++;
		}
		catch (Exception exception)
		{
			_logger.LogError($"Failed to convert: {ConvertProgressText}", exception);
		}
	}

	private async Task DeleteFile(FolderStructure file)
	{
		await Task.Run(() =>
		{
			try
			{
				RemoveProgressText = Path.GetFileName(file.Path);
				_logger.LogInformation($"Remove: {RemoveProgressText}");

				File.Delete(file.Path);
				RemoveProgress++;
			}
			catch (Exception exception)
			{
				_logger.LogError($"Failed to remove: {RemoveProgressText}", exception);
			}
		});
	}
}
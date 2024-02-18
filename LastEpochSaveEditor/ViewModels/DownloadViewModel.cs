using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LastEpochSaveEditor.Models.Database;
using LastEpochSaveEditor.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace LastEpochSaveEditor.ViewModels
{
	internal partial class DownloadViewModel : ObservableObject
	{
		private const string IMAGE_FOLDER_NAME = "Images";
		private const string BASIC_FOLDER_NAME = "Basic";
		private const string SET_FOLDER_NAME = "Set";
		private const string UNIQUE_FOLDER_NAME = "Uniques";
		
		private readonly ILogger<DownloadViewModel> _logger;
		private readonly IDB _db;

		#region Properties

		[ObservableProperty]
		private int _downloadProgress;

		[ObservableProperty]
		private int _convertProgress;

		[ObservableProperty]
		private string _downloadProgressText;

		[ObservableProperty]
		private string _convertProgressText;

		[ObservableProperty]
		private int _count;

		[ObservableProperty]
		private bool _canDownload;

		#endregion

		#region Command

		[RelayCommand]
		private void Dowload()
		{
			Count = _db.Count();
			CanDownload = false;
			PrepareFolders();
			DownloadImages();
		}

		#endregion

		public DownloadViewModel(ILogger<DownloadViewModel> logger, IDB db)
		{
			_logger = logger;
			_db = db;

			Count = int.MaxValue;
			CanDownload = true;
		}

		private void PrepareFolders()
		{
			if (Directory.Exists(IMAGE_FOLDER_NAME))
				Directory.Delete(IMAGE_FOLDER_NAME, true);
			Directory.CreateDirectory(IMAGE_FOLDER_NAME);

			string rootPath;
			string path;
			string folderPath;
			foreach (var item in _db.GetFolderStructure())
			{
				rootPath = Path.Combine(IMAGE_FOLDER_NAME, item.Key);
				if (!Directory.Exists(rootPath))
					Directory.CreateDirectory(rootPath);

				foreach (var type in item.Value)
				{
					var name = type.Base.BaseTypeName;
					path = Path.Combine(rootPath, name);
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);

					folderPath = Path.Combine(path, BASIC_FOLDER_NAME);
					Directory.CreateDirectory(folderPath);

					folderPath = Path.Combine(path, UNIQUE_FOLDER_NAME);
					Directory.CreateDirectory(folderPath);

					folderPath = Path.Combine(path, SET_FOLDER_NAME);
					Directory.CreateDirectory(folderPath);
				}
			}
		}

		private void DownloadImages()
		{
			var allItems = _db.GetAll();
			DownloadProgress = 0;
			ConvertProgress = 0;

			var files = GetFilesForDownload(allItems);
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += async (s, e) =>
			{
				foreach (var item in files)
				{
					var filePath = item;
					

					foreach (var path in item.PathList)
					{
						try
						{
							DownloadProgressText = Path.GetFileName(path);
							_logger.LogInformation($"Download: {DownloadProgressText}");

							await FileDownloader.DownloadFile(item.Type, path);
							DownloadProgress++;
						}
						catch (Exception exception)
						{
							_logger.LogError($"Failed to download: {DownloadProgressText}", exception);
						}
					}

					/*foreach (var path in item.PathList)
					{
						try
						{
							ConvertProgressText = Path.GetFileName(path);
							_logger.LogInformation($"Convert: {ConvertProgressText}");

							await WebPConverter.Convert(path);
							ConvertProgress++;
						}
						catch (Exception exception)
						{
							_logger.LogError($"Failed to convert: {ConvertProgressText}", exception);
						}
					}*/
				}

				CanDownload = true;
			};
			
			worker.RunWorkerAsync();
		}

		private IEnumerable<FolderStructure> GetFilesForDownload(IEnumerable<Item> allItems)
		{
			string rootPath;
			string path;
			string folderPath;
			string fileName;
			var result = new List<FolderStructure>(3);
			var baseItems = new List<Item>();
			var uniques = new List<string>();
			foreach (var item in _db.GetFolderStructure())
			{
				rootPath = Path.Combine(IMAGE_FOLDER_NAME, item.Key);
				foreach (var type in item.Value)
				{
					var name = type.Base.BaseTypeName;
					var pathList = new List<string>(type.Base.SubItems.Count + type.Uniques.Count() + type.Sets.Count());
					path = Path.Combine(rootPath, name);

					folderPath = Path.Combine(path, BASIC_FOLDER_NAME);
					foreach (var baseItem in type.Base.SubItems)
					{
						fileName = baseItem.Name.ToLowerInvariant().Replace(" ", "_");
						pathList.Add($"{Path.Combine(folderPath, fileName)}.webp");
					}

					folderPath = Path.Combine(path, UNIQUE_FOLDER_NAME);
					foreach (var unique in type.Uniques)
					{
						fileName = unique.Name.ToLowerInvariant().Replace(" ", "_");
						pathList.Add($"{Path.Combine(folderPath, fileName)}.webp");
					}

					folderPath = Path.Combine(path, SET_FOLDER_NAME);
					foreach (var set in type.Sets)
					{
						fileName = set.Name.ToLowerInvariant().Replace(" ", "_");
						pathList.Add($"{Path.Combine(folderPath, fileName)}.webp");
					}

					result.Add(new FolderStructure(name.ToLowerInvariant().Replace(" ", "_"), pathList));
				}
			}

			/*result.Add(new(BASIC_FOLDER_NAME, baseItems));
			result.Add(new(UNIQUE_FOLDER_NAME, uni));*/
			return result;
		}
	}
}

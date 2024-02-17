using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LastEpochSaveEditor.Models.Database;
using LastEpochSaveEditor.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LastEpochSaveEditor.ViewModels
{
	internal partial class DownloadViewModel : ObservableObject
	{
		private const string IMAGE_FOLDER_NAME = "Images";
		private const string BASIC_FOLDER_NAME = "Basic";
		private const string SET_FOLDER_NAME = "Set";
		private const string UNIQUE_FOLDER_NAME = "Unique";
		
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

		#endregion

		#region Command

		[RelayCommand]
		private void Dowload()
		{
			Count = _db.Count();
			PrepareFolders();
			DownloadImages();
		}

		#endregion

		public DownloadViewModel(ILogger<DownloadViewModel> logger, IDB db)
		{
			_logger = logger;
			_db = db;

			Count = int.MaxValue;
		}

		private void PrepareFolders()
		{
			if (Directory.Exists(IMAGE_FOLDER_NAME))
				Directory.Delete(IMAGE_FOLDER_NAME, true);
			Directory.CreateDirectory(IMAGE_FOLDER_NAME);

			string rootPath;
			string path;
			foreach (var item in _db.GetFolderStructure())
			{
				throw new System.NotImplementedException();
			}
			//foreach (var folder in _db!.GetGroupNames())
			//{
			//	rootPath = Path.Combine(IMAGE_FOLDER_NAME, folder);
			//	Directory.CreateDirectory(rootPath);

			//	path = Path.Combine(rootPath, BASIC_FOLDER_NAME);
			//	Directory.CreateDirectory(path);

			//	path = Path.Combine(rootPath, UNIQUE_FOLDER_NAME);
			//	Directory.CreateDirectory(path);

			//	path = Path.Combine(rootPath, SET_FOLDER_NAME);
			//	Directory.CreateDirectory(path);
			//}
		}

		private void DownloadImages()
		{
			var allItems = _db.GetAll();

			SetDownloadCount(allItems);
			//DownloadHelmets();
		}

		/*private void DownloadHelmets()
		{
			var helmets = _db.GetHelmets();

		}*/

		private void SetDownloadCount(IEnumerable<Item> items)
		{
			if (items?.Any() == false)
			{
				Count = 0;
				return;
			}

			Count = 0;
			foreach (var item in items!)
				Count += item.Base.SubItems.Count + item.Uniques.Count() + item.Sets.Count();
		}
	}
}

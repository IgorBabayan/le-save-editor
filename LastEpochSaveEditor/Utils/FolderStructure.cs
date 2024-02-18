using System.Collections.Generic;

namespace LastEpochSaveEditor.Utils
{
	internal record class FolderStructure(string Type, IEnumerable<string> PathList);
}

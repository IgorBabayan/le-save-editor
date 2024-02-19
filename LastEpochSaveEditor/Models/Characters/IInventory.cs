using System.Collections.Generic;

namespace LastEpochSaveEditor.Models.Characters
{
	public interface IInventory
	{
		void Parse(IDictionary<int, List<int>> data);
	}
}

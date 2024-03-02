namespace LastEpochSaveEditor.Models.Characters;

public interface IInventory
{
	Task Parse(IDictionary<int, List<int>> data);
}
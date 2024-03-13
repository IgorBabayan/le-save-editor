namespace LastEpochSaveEditor.Utils;

public interface ICloneable<out TObject> : ICloneable
	where TObject : class, new()
{
	new TObject Clone();
}
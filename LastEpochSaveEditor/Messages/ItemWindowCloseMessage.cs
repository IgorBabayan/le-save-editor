namespace LastEpochSaveEditor.Messages;

public class ItemWindowCloseMessage : ValueChangedMessage<Guid>
{
	public ItemWindowCloseMessage(Guid value)
		: base(value) { }
}
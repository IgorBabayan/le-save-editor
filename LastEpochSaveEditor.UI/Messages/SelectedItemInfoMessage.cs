namespace LastEpochSaveEditor.Messages;

internal class SelectedItemInfoMessage(Character value, ItemInfoTypeEnum infoType)
	: ValueChangedMessage<Character>(value)
{
	public ItemInfoTypeEnum InfoType { get; private set; } = infoType;
}

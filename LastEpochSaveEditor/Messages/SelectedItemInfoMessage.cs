namespace LastEpochSaveEditor.Messages;

internal class SelectedItemInfoMessage : ValueChangedMessage<Character>
{
	public ItemInfoTypeEnum InfoType { get; private set; }

	public SelectedItemInfoMessage(Character value)
		: this(value, ItemInfoTypeEnum.All) { }

	public SelectedItemInfoMessage(Character value, ItemInfoTypeEnum infoType)
		: base(value) => InfoType = infoType;
}
namespace LastEpochSaveEditor.Messages;

public class ItemSelectedMessage : ValueChangedMessage<ItemSelectedValue>
{
	public ItemSelectedMessage(ItemSelectedValue value)
		: base(value) { }
}

public record ItemSelectedValue(Guid Id, ItemDataInfo Value);
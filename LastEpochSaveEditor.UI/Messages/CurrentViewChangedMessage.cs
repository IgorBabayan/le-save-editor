namespace LastEpochSaveEditor.Messages;

internal class CurrentViewChangedMessage : ValueChangedMessage<ObservableObject>
{
	public CurrentViewChangedMessage(ObservableObject value)
		: base(value) { }
}
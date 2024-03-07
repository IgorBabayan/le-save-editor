namespace LastEpochSaveEditor.Messages;

internal class CurrentViewChangedMessage(ObservableObject value) : ValueChangedMessage<ObservableObject>(value);
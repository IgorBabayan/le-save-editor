namespace LastEpochSaveEditor.Messages;

public class CloseCurrentPopupMessage : ValueChangedMessage<bool>
{
    public CloseCurrentPopupMessage(bool value)
        : base(value) { }
}
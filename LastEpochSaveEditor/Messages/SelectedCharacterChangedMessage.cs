namespace LastEpochSaveEditor.Messages;

internal class SelectedCharacterChangedMessage : ValueChangedMessage<CharacterInfo>
{
	public SelectedCharacterChangedMessage(CharacterInfo character)
		: base(character) { }
}
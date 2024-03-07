namespace LastEpochSaveEditor.Messages;

internal class SelectedCharacterChangedMessage(CharacterInfo? character) : ValueChangedMessage<CharacterInfo?>(character);
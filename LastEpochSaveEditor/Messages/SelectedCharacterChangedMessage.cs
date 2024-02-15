using CommunityToolkit.Mvvm.Messaging.Messages;
using LastEpochSaveEditor.Models;

namespace LastEpochSaveEditor.Messages
{
	internal class SelectedCharacterChangedMessage : ValueChangedMessage<CharacterInfo>
	{
        public SelectedCharacterChangedMessage(CharacterInfo character)
            : base(character) { }
    }
}

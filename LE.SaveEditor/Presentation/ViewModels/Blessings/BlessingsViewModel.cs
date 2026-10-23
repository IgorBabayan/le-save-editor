using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Blessings;

/// <summary>Blessings of one character. Not parsed yet, so the tab is empty for now.</summary>
public sealed class BlessingsViewModel(CharacterSave character) : ViewModelBase
{
    public CharacterSave Character { get; } = character;
}

using System.Collections.ObjectModel;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>Inventory of one character. Items are not parsed yet, so the list is always empty for now.</summary>
public sealed class InventoryViewModel(CharacterSave character) : ViewModelBase
{
    public CharacterSave Character { get; } = character;

    public ObservableCollection<object> Items { get; } = [];

    public bool IsEmpty => Items.Count == 0;
}

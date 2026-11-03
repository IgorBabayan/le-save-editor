using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Factions;

namespace LE.SaveEditor.Presentation.ViewModels.Factions;

/// <summary>
/// One faction of the "Factions" section: locked (greyed out) until a character of the stash has unlocked it in game,
/// otherwise its rank with a slider over 0 … <see cref="Faction.MaxRank"/>. The rank is kept in the stash file, so it is
/// shared by every character using that stash and written when the character is saved.
/// </summary>
public sealed partial class FactionViewModel : ViewModelBase
{
    private readonly StashMaterialsViewModel? _stash;

    /// <summary>Set while the rank is filled from the stash, so it is not written back.</summary>
    private bool _updating;

    public FactionViewModel(Faction faction, StashMaterialsViewModel? stash)
    {
        Faction = faction;
        _stash = stash;
        Refresh();
    }

    public Faction Faction { get; }

    public string Name => Faction.Name;

    /// <summary>Highest rank: the slider's maximum.</summary>
    public int MaxRank => Faction.MaxRank;

    /// <summary>The stash has an entry for the faction: a character of the stash has unlocked it in game.</summary>
    public bool IsUnlocked => _stash?.HasFaction(Faction.Id) == true;

    /// <summary>Rank of the faction with the change made in the editor (0 while locked).</summary>
    [ObservableProperty]
    private int _rank;

    /// <summary>The rank differs from the stash file.</summary>
    public bool HasChanges =>
        IsUnlocked && _stash!.Stash.FactionRanks.GetValueOrDefault(Faction.Id) != Rank;

    /// <summary>"Rank 4 of 12", or why the faction cannot be edited.</summary>
    public string StatusText => IsUnlocked
        ? $"Rank {Rank} of {MaxRank}"
        : _stash is null
            ? "No stash file was found for this character."
            : "Locked: not unlocked in game yet.";

    /// <summary>Tooltip of the faction row: where the rank is kept.</summary>
    public string ToolTip => _stash switch
    {
        null => "No stash file was found for this character in the save folder, so its factions cannot be edited.",
        _ when !IsUnlocked =>
            $"{_stash.Stash.FileName} has no {Name} entry yet: unlock the faction in game first.",
        var stash =>
            $"{Name} rank kept in {stash.Stash.FileName}, shared by every character using that stash. " +
            "Changing it resets the reputation towards the next rank. Written when the character is saved."
    };

    partial void OnRankChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, MaxRank);
        if (clamped != value)
        {
            Rank = clamped;
            return;
        }

        if (!_updating)
            _stash?.SetFactionRank(Faction.Id, value, MaxRank);

        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasChanges));
    }

    /// <summary>Shows the rank the stash holds now (another character of the stash may have changed it, or it was saved).</summary>
    public void Refresh()
    {
        _updating = true;
        try
        {
            Rank = Math.Clamp(_stash?.GetFactionRank(Faction.Id) ?? 0, 0, MaxRank);
        }
        finally
        {
            _updating = false;
        }

        OnPropertyChanged(nameof(IsUnlocked));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ToolTip));
        OnPropertyChanged(nameof(HasChanges));
    }
}

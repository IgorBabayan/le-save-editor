using System.Collections.ObjectModel;
using LE.SaveEditor.Services.GameData.Waypoints;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Waypoints;

/// <summary>
/// "Waypoints" tab: which waypoints the character has unlocked (<c>unlockedWaypointScenes</c>), grouped by chapter.
/// Waypoints are unlocked or locked one by one, per chapter, or all at once; <see cref="GetChangedScenes"/> gives what
/// to write when the character is saved.
/// </summary>
public sealed partial class WaypointsViewModel : ViewModelBase
{
    /// <summary>Group of scenes in the save that the editor does not know; they are listed so they can be locked.</summary>
    public const string UnknownGroup = "Other scenes in the save";

    private readonly IReadOnlyList<string> _savedScenes;
    private readonly HashSet<string> _savedSet;
    private bool _isBatchUpdate;

    public WaypointsViewModel(CharacterSave character)
    {
        Character = character;
        _savedScenes = character.UnlockedWaypointScenes;
        _savedSet = new HashSet<string>(_savedScenes, StringComparer.Ordinal);

        var items = WaypointCatalog.All
            .Select(w => new WaypointItemViewModel(w.Scene, w.Name, w.Chapter, _savedSet.Contains(w.Scene), isKnown: true))
            .ToList();

        // Scenes the editor does not know stay in the file unless they are locked here.
        items.AddRange(_savedScenes
            .Where(s => WaypointCatalog.Find(s) is null)
            .Distinct(StringComparer.Ordinal)
            .Select(s => new WaypointItemViewModel(s, s, UnknownGroup, isUnlocked: true, isKnown: false)));

        Items = items;
        foreach (var item in items)
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WaypointItemViewModel.IsUnlocked) && !_isBatchUpdate)
                    OnWaypointsChanged();
            };

        Groups = new ObservableCollection<WaypointGroupViewModel>(
            items.GroupBy(i => i.Group).Select(g => new WaypointGroupViewModel(g.Key, g.ToList(), this)));
    }

    public CharacterSave Character { get; }

    /// <summary>Every waypoint listed, in the order of <see cref="Groups"/>.</summary>
    public IReadOnlyList<WaypointItemViewModel> Items { get; }

    public ObservableCollection<WaypointGroupViewModel> Groups { get; }

    /// <summary>Raised when a waypoint was unlocked or locked.</summary>
    public event EventHandler? Changed;

    public int UnlockedCount => Items.Count(i => i.IsUnlocked);

    /// <summary>"12 of 92 waypoints unlocked".</summary>
    public string Summary => $"{UnlockedCount} of {Items.Count} waypoints unlocked";

    /// <summary>The unlocked waypoints differ from the save file.</summary>
    public bool HasChanges => Items.Any(i => i.IsUnlocked != _savedSet.Contains(i.Scene));

    /// <summary>
    /// The scenes to write into <c>unlockedWaypointScenes</c>; <c>null</c> when nothing changed. Scenes already in the
    /// file keep their order, newly unlocked ones follow in map order.
    /// </summary>
    public IReadOnlyList<string>? GetChangedScenes()
    {
        if (!HasChanges)
            return null;

        var unlocked = Items.Where(i => i.IsUnlocked).Select(i => i.Scene).ToHashSet(StringComparer.Ordinal);
        var scenes = _savedScenes.Where(unlocked.Contains).Distinct(StringComparer.Ordinal).ToList();
        scenes.AddRange(Items.Where(i => i.IsUnlocked && !_savedSet.Contains(i.Scene)).Select(i => i.Scene));
        return scenes;
    }

    [RelayCommand]
    private void UnlockAll() => SetUnlocked(Items, true);

    [RelayCommand]
    private void LockAll() => SetUnlocked(Items, false);

    /// <summary>Back to the waypoints of the save file.</summary>
    [RelayCommand]
    private void Revert()
    {
        _isBatchUpdate = true;
        try
        {
            foreach (var item in Items)
                item.IsUnlocked = _savedSet.Contains(item.Scene);
        }
        finally
        {
            _isBatchUpdate = false;
        }

        OnWaypointsChanged();
    }

    /// <summary>Unlocks or locks <paramref name="items"/>, reporting the change once.</summary>
    internal void SetUnlocked(IEnumerable<WaypointItemViewModel> items, bool unlocked)
    {
        _isBatchUpdate = true;
        try
        {
            foreach (var item in items)
                item.IsUnlocked = unlocked;
        }
        finally
        {
            _isBatchUpdate = false;
        }

        OnWaypointsChanged();
    }

    private void OnWaypointsChanged()
    {
        foreach (var group in Groups)
            group.Refresh();

        OnPropertyChanged(nameof(UnlockedCount));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(HasChanges));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>The waypoints of one chapter (or of the End of Time, Monolith, arena and dungeons).</summary>
public sealed partial class WaypointGroupViewModel : ViewModelBase
{
    private readonly WaypointsViewModel _owner;

    public WaypointGroupViewModel(string name, IReadOnlyList<WaypointItemViewModel> items, WaypointsViewModel owner)
    {
        Name = name;
        Items = items;
        _owner = owner;
    }

    public string Name { get; }

    public IReadOnlyList<WaypointItemViewModel> Items { get; }

    /// <summary>"3 / 7".</summary>
    public string Progress => $"{Items.Count(i => i.IsUnlocked)} / {Items.Count}";

    [RelayCommand]
    private void UnlockAll() => _owner.SetUnlocked(Items, true);

    [RelayCommand]
    private void LockAll() => _owner.SetUnlocked(Items, false);

    internal void Refresh() => OnPropertyChanged(nameof(Progress));
}

/// <summary>One waypoint: a check box that unlocks it.</summary>
public sealed partial class WaypointItemViewModel : ViewModelBase
{
    public WaypointItemViewModel(string scene, string name, string group, bool isUnlocked, bool isKnown)
    {
        Scene = scene;
        Name = name;
        Group = group;
        IsKnown = isKnown;
        IsUnlocked = isUnlocked;
    }

    /// <summary>Scene name stored in the save.</summary>
    public string Scene { get; }

    /// <summary>Zone name (the scene name for a scene the editor does not know).</summary>
    public string Name { get; }

    public string Group { get; }

    /// <summary>The scene is in <see cref="WaypointCatalog"/>.</summary>
    public bool IsKnown { get; }

    public string ToolTip => IsKnown
        ? $"Scene {Scene}"
        : $"Scene {Scene} is not in the editor's list of waypoints. It stays in the save unless it is unchecked.";

    [ObservableProperty]
    private bool _isUnlocked;
}

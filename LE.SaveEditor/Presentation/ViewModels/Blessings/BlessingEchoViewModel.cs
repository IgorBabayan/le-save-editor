using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Blessings;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Blessings;

/// <summary>
/// The blessing slot of one timeline: the blessing the character holds for it (<see cref="Selected"/>, with its rolled
/// stats in <see cref="Stats"/>) and the blessings the timeline can grant (<see cref="Options"/>).
/// </summary>
public sealed partial class BlessingEchoViewModel : ViewModelBase
{
    public const string NormalGroup = "Blessings";
    public const string GrandGroup = "Grand blessings (empowered timeline)";
    public const string OtherGroup = "Other";

    /// <summary>The <c>savedItems</c> entry the slot was read from; <c>null</c> when the slot was empty.</summary>
    private readonly SavedItem? _source;

    /// <summary>The decoded <see cref="_source"/>, kept to write the blessing back in its own format.</summary>
    private readonly ItemData? _loaded;

    /// <summary>The option that matches the blessing in the save; <c>null</c> when the slot was empty.</summary>
    private readonly BlessingOptionViewModel? _loadedOption;

    private bool _isRestoring;

    public BlessingEchoViewModel(Timeline timeline, SavedItem? source, IItemBaseCatalog catalog, IIconProvider iconProvider)
    {
        Timeline = timeline;
        _source = source;
        _loaded = source is null ? null : ItemDataReader.Read(source.Data);

        var options = timeline.Blessings
            .Select(b => new BlessingOptionViewModel(b.Base, b.IsGrand ? GrandGroup : NormalGroup, iconProvider))
            .ToList();

        if (source is not null && _loaded is null)
        {
            // Stored in a shape the editor cannot read: kept as it is unless another blessing is picked or it is removed.
            _loadedOption = new BlessingOptionViewModel(
                new ItemBase(Timeline.BlessingBaseTypeId, -1, "Unreadable blessing", "Blessing", 0, 0, false, null, []),
                OtherGroup, iconProvider);
            options.Add(_loadedOption);
        }
        else if (_loaded is { } loaded)
        {
            _loadedOption = options.FirstOrDefault(o => o.Base.SubTypeId == loaded.SubTypeId);
            if (_loadedOption is null)
            {
                // A blessing of another timeline, or one the database does not know: listed so it is kept or can be
                // picked again after trying others.
                var known = catalog.FindBase(Timeline.BlessingBaseTypeId, loaded.SubTypeId);
                var blessing = known ?? new ItemBase(Timeline.BlessingBaseTypeId, loaded.SubTypeId,
                    $"Unknown blessing ({loaded.SubTypeId})", "Blessing", 0, 0, false, null, []);
                _loadedOption = new BlessingOptionViewModel(blessing, OtherGroup, iconProvider);
                options.Add(_loadedOption);
            }
        }

        Options = options;
        var view = new ListCollectionView(options);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BlessingOptionViewModel.Group)));
        OptionsView = view;

        _isRestoring = true;
        try
        {
            Selected = _loadedOption;
            RestoreLoadedRolls();
        }
        finally
        {
            _isRestoring = false;
        }
    }

    public Timeline Timeline { get; }

    /// <summary>Timeline name, e.g. "Fall of the Outcasts".</summary>
    public string Name => Timeline.Name;

    /// <summary>"Level 62 · empowered 100".</summary>
    public string LevelText => Timeline.Level > 0 ? $"Level {Timeline.Level} · empowered 100" : "Empowered 100";

    /// <summary>Every blessing that can be picked, the blessing in the save included.</summary>
    public IReadOnlyList<BlessingOptionViewModel> Options { get; }

    /// <summary><see cref="Options"/> grouped into normal, Grand and other blessings, for the list.</summary>
    public ICollectionView OptionsView { get; }

    /// <summary>The blessing in the slot; <c>null</c> for an empty slot.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBlessing))]
    [NotifyPropertyChangedFor(nameof(SelectedName))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(HasIcon))]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    private BlessingOptionViewModel? _selected;

    /// <summary>Rolled stats of <see cref="Selected"/>, each with its slider.</summary>
    public ObservableCollection<ItemStatViewModel> Stats { get; } = [];

    public bool HasBlessing => Selected is not null;

    /// <summary>The blessing name, or "No blessing".</summary>
    public string SelectedName => Selected?.Name ?? "No blessing";

    public ImageSource? Icon => Selected?.Icon;

    public bool HasIcon => Icon is not null;

    /// <summary>The slot differs from the save file.</summary>
    public bool HasChanges
    {
        get
        {
            try
            {
                return GetUpdate() is not null;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }

    /// <summary>A blessing was picked or removed, or a roll changed.</summary>
    public event EventHandler? Changed;

    /// <summary>What to write for this slot; <c>null</c> when it matches the save.</summary>
    /// <exception cref="ArgumentException">The blessing cannot be stored.</exception>
    public SavedItemUpdate? GetUpdate()
    {
        if (Selected is not { } selected)
            return _source is null ? null : SavedItemUpdate.Remove(_source);

        // The unreadable blessing of the save, still selected: nothing to write.
        if (_source is not null && _loaded is null && ReferenceEquals(selected, _loadedOption))
            return null;

        var data = ItemDataWriter.Write(BuildItemData(selected));
        if (_source is null)
        {
            return SavedItemUpdate.Add(new SavedItem(
                Timeline.ContainerId, 0, 0, 1, ItemDataReader.Version2, data));
        }

        return data.SequenceEqual(_source.Data) ? null : SavedItemUpdate.Replace(_source, _source with { Data = data });
    }

    /// <summary>Puts <paramref name="option"/> (one of this echo's <see cref="Options"/>) in the slot.</summary>
    [RelayCommand]
    private void Pick(BlessingOptionViewModel? option)
    {
        if (option is not null && Options.Contains(option))
            Selected = option;
    }

    /// <summary>Empties the slot.</summary>
    [RelayCommand(CanExecute = nameof(HasBlessing))]
    private void Remove() => Selected = null;

    /// <summary>Back to the blessing (and rolls) of the save file.</summary>
    [RelayCommand]
    private void Revert()
    {
        _isRestoring = true;
        try
        {
            Selected = _loadedOption;
            RestoreLoadedRolls();
        }
        finally
        {
            _isRestoring = false;
        }

        RaiseChanged();
    }

    partial void OnSelectedChanged(BlessingOptionViewModel? oldValue, BlessingOptionViewModel? newValue)
    {
        // The highlighted row of the list.
        if (oldValue is not null)
            oldValue.IsSelected = false;
        if (newValue is not null)
            newValue.IsSelected = true;
    }

    partial void OnSelectedChanged(BlessingOptionViewModel? value)
    {
        foreach (var stat in Stats)
            stat.PropertyChanged -= OnStatChanged;
        Stats.Clear();

        // A new blessing starts at its best rolls (ItemStatViewModel's default).
        if (value is not null)
        {
            foreach (var implicitStat in value.Base.Implicits.Take(ItemDataReader.ImplicitCount))
            {
                var stat = new ItemStatViewModel(implicitStat);
                stat.PropertyChanged += OnStatChanged;
                Stats.Add(stat);
            }
        }

        if (!_isRestoring)
            RaiseChanged();
    }

    private void OnStatChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ItemStatViewModel.Roll) && !_isRestoring)
            RaiseChanged();
    }

    private void RaiseChanged()
    {
        OnPropertyChanged(nameof(HasChanges));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Puts the rolls stored in the save on the sliders, when the blessing of the save is selected.</summary>
    private void RestoreLoadedRolls()
    {
        if (_loaded is null || !ReferenceEquals(Selected, _loadedOption))
            return;

        for (var i = 0; i < Stats.Count && i < _loaded.ImplicitRolls.Count; i++)
            Stats[i].Roll = _loaded.ImplicitRolls[i];
    }

    /// <summary>
    /// The blessing as a plain item: no affixes, its rolls from the sliders. A blessing that replaces one from the save
    /// keeps that entry's format (version, instance id, flags, closing byte); rolls of implicits the editor does not
    /// show stay as they were stored.
    /// </summary>
    private ItemData BuildItemData(BlessingOptionViewModel selected)
    {
        var keepStoredRolls = _loaded is not null && _loaded.SubTypeId == selected.Base.SubTypeId;
        var rolls = new byte[ItemDataReader.ImplicitCount];
        for (var i = 0; i < rolls.Length; i++)
        {
            rolls[i] = i < Stats.Count ? (byte)Math.Clamp(Math.Round(Stats[i].Roll), 0, byte.MaxValue)
                : keepStoredRolls && i < _loaded!.ImplicitRolls.Count ? _loaded.ImplicitRolls[i]
                : byte.MaxValue;
        }

        if (_loaded is { } loaded)
        {
            return loaded with
            {
                BaseTypeId = Timeline.BlessingBaseTypeId,
                SubTypeId = selected.Base.SubTypeId,
                ImplicitRolls = rolls,
                // Blessings carry no affixes; keep what the game stored around them.
                UniqueId = null,
                UniqueRolls = [],
                RarityCode = keepStoredRolls ? loaded.RarityCode : 0,
                ForgingPotential = loaded.ForgingPotential ?? 0,
                Affixes = keepStoredRolls ? loaded.Affixes : []
            };
        }

        // Like the game writes a blessing: [2, 34, subTypeID, 0, 0, rolls ×3, 0, 0, 0].
        return new ItemData(Timeline.BlessingBaseTypeId, selected.Base.SubTypeId, 0, rolls, null, [], 0, [],
            ItemDataReader.Version2, null, 0, 0);
    }
}

/// <summary>A blessing that can be picked for an echo.</summary>
public sealed partial class BlessingOptionViewModel(ItemBase blessing, string group, IIconProvider iconProvider) : ViewModelBase
{
    /// <summary>The option is the blessing in the echo's slot (highlighted in the list).</summary>
    [ObservableProperty]
    private bool _isSelected;

    public ItemBase Base { get; } = blessing;

    /// <summary><see cref="BlessingEchoViewModel.NormalGroup"/>, <see cref="BlessingEchoViewModel.GrandGroup"/> or <see cref="BlessingEchoViewModel.OtherGroup"/>.</summary>
    public string Group { get; } = group;

    public string Name => Base.Name;

    public ImageSource? Icon => iconProvider.Get(Base.IconPath);

    public bool HasIcon => Icon is not null;

    /// <summary>The stats with their roll ranges, e.g. "+(10–15)% increased Item Rarity"; one line per stat.</summary>
    public string StatsText => Base.Implicits.Count == 0
        ? "No stats known"
        : string.Join(Environment.NewLine, Base.Implicits.Select(ItemStatViewModel.FormatRange));
}

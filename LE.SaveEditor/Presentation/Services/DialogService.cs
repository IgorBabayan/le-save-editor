using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Presentation.Views.Database;
using LE.SaveEditor.Presentation.Views.Dialogs;
using LE.SaveEditor.Presentation.Views.Inventory;
using LE.SaveEditor.Presentation.Views.Settings;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.Services;

public sealed class DialogService(IServiceProvider services) : IDialogService
{
    public bool ShowSettings()
    {
        var window = services.GetRequiredService<SettingsWindow>();
        return WindowHelper.ShowModal(window) is true;
    }

    public bool Confirm(string title, string message)
    {
        var window = new ConfirmationWindow(title, message);
        return WindowHelper.ShowModal(window) is true;
    }

    public DatabaseGenerationResult? ShowDatabaseGenerator()
    {
        var window = services.GetRequiredService<DatabaseGeneratorWindow>();
        WindowHelper.ShowModal(window);
        return window.Result;
    }

    public BuiltItem? ShowItemBuilder(EquipmentSlot slot, CharacterClass characterClass, BuiltItem? current) =>
        ShowItemBuilderWindow(CreateItemBuilder(slot, characterClass, current));

    public void ShowItemInspector(EquipmentSlotViewModel slot, CharacterClass characterClass)
    {
        // Items kept in a grid search the extra slots too (a backpack weapon search finds off-hands).
        var viewModel = CreateItemBuilder(slot.Slot, characterClass, slot.Item, slot.IsInGrid ? slot.Slot.ExtraSearchSlots : null);
        // Removing empties the slot; a backpack / idol grid item then leaves its grid.
        viewModel.RemoveRequested += (_, _) => slot.Item = null;

        if (FindSlotControl(slot) is { } anchor
            && ItemInspectorAdorner.TryShow(anchor, viewModel, item => slot.Item = item, vm => ShowItemBuilderWindow(vm)))
            return;

        if (ShowItemBuilderWindow(viewModel) is { } built)
            slot.Item = built;
    }

    public void ShowItemInspector(EquipmentSlot slot, CharacterClass characterClass, object? anchor, Action<BuiltItem> applied)
    {
        // New items are only built in grids (backpack, idol grid).
        var viewModel = CreateItemBuilder(slot, characterClass, null, slot.ExtraSearchSlots);

        if (anchor is Control element
            && ItemInspectorAdorner.TryShow(element, viewModel, applied, vm => ShowItemBuilderWindow(vm)))
            return;

        if (ShowItemBuilderWindow(viewModel) is { } built)
            applied(built);
    }

    public CraftingMaterial? ShowCraftingMaterialPicker(CraftingMaterialKind kind, CraftingMaterial? current)
    {
        var viewModel = new CraftingMaterialPickerViewModel(
            services.GetRequiredService<IItemBaseCatalog>(),
            services.GetRequiredService<IIconProvider>(),
            kind,
            current);
        var window = new CraftingMaterialPickerWindow(viewModel);
        WindowHelper.ShowModal(window);
        return window.Result;
    }

        public IReadOnlyDictionary<int, int>? ShowCraftingMaterialStorage(
        CraftingMaterialKind kind,
        IReadOnlyDictionary<int, int> amounts,
        string stashName)
    {
        var viewModel = new CraftingMaterialStorageViewModel(
            services.GetRequiredService<IItemBaseCatalog>(),
            services.GetRequiredService<IIconProvider>(),
            k => ShowCraftingMaterialPicker(k, null),
            kind,
            amounts,
            stashName);
        var window = new CraftingMaterialStorageWindow(viewModel);
        WindowHelper.ShowModal(window);
        return window.Result;
    }

    private ItemBuilderViewModel CreateItemBuilder(
        EquipmentSlot slot,
        CharacterClass characterClass,
        BuiltItem? current,
        IReadOnlyList<EquipmentSlot>? extraSearchSlots = null) =>
        new(services.GetRequiredService<IItemBaseCatalog>(),
            services.GetRequiredService<IIconProvider>(),
            Confirm,
            slot,
            characterClass,
            current,
            extraSearchSlots);

    private static BuiltItem? ShowItemBuilderWindow(ItemBuilderViewModel viewModel)
    {
        var window = new ItemBuilderWindow(viewModel);
        WindowHelper.ShowModal(window);
        return window.Result;
    }

    /// <summary>
    /// The button that was just used to open <paramref name="slot"/>: the focused element (a clicked button takes
    /// the focus) or the one under the mouse, whichever belongs to a button bound to the slot.
    /// </summary>
    private static Control? FindSlotControl(EquipmentSlotViewModel slot)
    {
        if (WindowHelper.GetActiveWindow() is not { } window)
            return null;

        // Buttons take the focus when clicked; the pointer is the fallback (e.g. a button that is not focusable).
        var starts = new[] { window.FocusManager?.GetFocusedElement() as Visual, window.IsPointerOver ? PointerOverVisual(window) : null };
        foreach (var start in starts)
        {
            for (var node = start; node is not null; node = node.GetVisualParent())
            {
                if (node is Button button && ReferenceEquals(button.DataContext, slot))
                    return button;
            }
        }

        return null;
    }

    /// <summary>Deepest control under the pointer, as last reported to the window.</summary>
    private static Visual? PointerOverVisual(Window window) =>
        window.GetVisualDescendants().OfType<InputElement>().LastOrDefault(e => e.IsPointerOver && e.IsEffectivelyVisible);
}

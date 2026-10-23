using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
        window.Owner = WindowHelper.GetActiveWindow();
        return window.ShowDialog() == true;
    }

    public bool Confirm(string title, string message)
    {
        var window = new ConfirmationWindow(title, message)
        {
            Owner = WindowHelper.GetActiveWindow()
        };
        return window.ShowDialog() == true;
    }

    public DatabaseGenerationResult? ShowDatabaseGenerator()
    {
        var window = services.GetRequiredService<DatabaseGeneratorWindow>();
        window.Owner = WindowHelper.GetActiveWindow();
        window.ShowDialog();
        return window.Result;
    }

    public BuiltItem? ShowItemBuilder(EquipmentSlot slot, CharacterClass characterClass, BuiltItem? current) =>
        ShowItemBuilderWindow(CreateItemBuilder(slot, characterClass, current));

    public void ShowItemInspector(EquipmentSlotViewModel slot, CharacterClass characterClass)
    {
        var viewModel = CreateItemBuilder(slot.Slot, characterClass, slot.Item);

        if (FindSlotControl(slot) is { } anchor
            && ItemInspectorAdorner.TryShow(anchor, viewModel, item => slot.Item = item, vm => ShowItemBuilderWindow(vm)))
            return;

        if (ShowItemBuilderWindow(viewModel) is { } built)
            slot.Item = built;
    }

    private ItemBuilderViewModel CreateItemBuilder(EquipmentSlot slot, CharacterClass characterClass, BuiltItem? current) =>
        new(services.GetRequiredService<IItemBaseCatalog>(),
            services.GetRequiredService<IIconProvider>(),
            Confirm,
            slot,
            characterClass,
            current);

    private static BuiltItem? ShowItemBuilderWindow(ItemBuilderViewModel viewModel)
    {
        var window = new ItemBuilderWindow(viewModel)
        {
            Owner = WindowHelper.GetActiveWindow()
        };
        window.ShowDialog();
        return window.Result;
    }

    /// <summary>
    /// The button that was just used to open <paramref name="slot"/>: the focused element (a clicked button takes
    /// the focus) or the one under the mouse, whichever belongs to a button bound to the slot.
    /// </summary>
    private static FrameworkElement? FindSlotControl(EquipmentSlotViewModel slot)
    {
        foreach (var start in new[] { Keyboard.FocusedElement as DependencyObject, Mouse.DirectlyOver as DependencyObject })
        {
            for (var node = start; node is not null; node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
            {
                if (node is ButtonBase button && ReferenceEquals(button.DataContext, slot))
                    return button;
            }
        }

        return null;
    }
}

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

    public BuiltItem? ShowItemBuilder(EquipmentSlot slot, CharacterClass characterClass, BuiltItem? current)
    {
        var viewModel = new ItemBuilderViewModel(
            services.GetRequiredService<IItemBaseCatalog>(),
            services.GetRequiredService<IIconProvider>(),
            slot,
            characterClass,
            current);

        var window = new ItemBuilderWindow(viewModel)
        {
            Owner = WindowHelper.GetActiveWindow()
        };
        window.ShowDialog();
        return window.Result;
    }
}

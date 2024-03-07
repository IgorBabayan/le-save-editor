global using System.IO;
global using System.Globalization;
global using System.ComponentModel;
global using System.Collections.Generic;
global using System.Net.Http;
global using System.Text;
global using System;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Windows.Input;
global using System.Collections;

global using Avalonia;
global using Avalonia.Platform;
global using Avalonia.Data.Converters;
global using Avalonia.Media;
global using Avalonia.Controls;
global using Avalonia.Controls.ApplicationLifetimes;
global using Avalonia.Markup.Xaml;
global using Avalonia.Media.Imaging;
global using Avalonia.Threading;
global using Avalonia.Input;
global using Avalonia.Controls.Primitives;

global using SixLabors.ImageSharp;

global using Newtonsoft.Json;

global using Serilog;
global using Serilog.Events;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using CommunityToolkit.Mvvm.Messaging;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using CommunityToolkit.Mvvm.Messaging.Messages;

global using LastEpochSaveEditor.ViewModels;
global using LastEpochSaveEditor.Models.Characters;
global using LastEpochSaveEditor.Models.CharacterModel;
global using LastEpochSaveEditor.Popups;
global using LastEpochSaveEditor.Views;
global using LastEpochSaveEditor.Messages;
global using LastEpochSaveEditor.Models.Database;
global using LastEpochSaveEditor.Data;
global using LastEpochSaveEditor.Utils;
global using LastEpochSaveEditor.Controls;
global using LastEpochSaveEditor.Models.Utils;
global using LastEpochSaveEditor.Services;
global using LastEpochSaveEditor.Services.Dialog;
global using LastEpochSaveEditor.Services.ItemInfo;
global using LastEpochSaveEditor.Services.Database;
global using LastEpochSaveEditor.Extensions;
global using LastEpochSaveEditor.Factories;

global using Color = Avalonia.Media.Color;
global using static LastEpochSaveEditor.Utils.Const;
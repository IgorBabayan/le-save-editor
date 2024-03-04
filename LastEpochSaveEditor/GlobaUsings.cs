global using System.Windows.Controls;
global using System.IO;
global using System.Globalization;
global using System.Windows.Data;
global using System.Windows.Media;
global using System.Windows.Media.Imaging;
global using System.ComponentModel;
global using System.Net.Http;
global using System.Text;
global using System.Windows.Input;
global using System.Collections;
global using System.Windows;

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
global using LastEpochSaveEditor.Utils;
global using LastEpochSaveEditor.Views;
global using LastEpochSaveEditor.Messages;
global using LastEpochSaveEditor.Models;
global using LastEpochSaveEditor.Models.Database;
global using LastEpochSaveEditor.Data;
global using LastEpochSaveEditor.Models.Utils;
global using LastEpochSaveEditor.Services;
global using LastEpochSaveEditor.Extensions;
global using LastEpochSaveEditor.Services.Factories;

global using Color = System.Windows.Media.Color;
global using static LastEpochSaveEditor.Utils.Const;
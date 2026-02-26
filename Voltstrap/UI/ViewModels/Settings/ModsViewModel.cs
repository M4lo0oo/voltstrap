using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;

using CommunityToolkit.Mvvm.Input;

using Voltstrap.Models.SettingTasks;
using Voltstrap.AppData;

namespace Voltstrap.UI.ViewModels.Settings
{
    public class ModsViewModel : NotifyPropertyChangedViewModel
    {
        private void OpenModsFolder() => Process.Start("explorer.exe", Paths.Modifications);

        private readonly Dictionary<string, byte[]> FontHeaders = new()
        {
            { "ttf", new byte[4] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[4] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[4] { 0x74, 0x74, 0x63, 0x66 } } 
        };

        private void ManageCustomFont()
        {
            if (!String.IsNullOrEmpty(TextFontTask.NewState))
            {
                TextFontTask.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.Menu_FontFiles}|*.ttf;*.otf;*.ttc"
                };

                if (dialog.ShowDialog() != true)
                    return;

                string type = dialog.FileName.Substring(dialog.FileName.Length-3, 3).ToLowerInvariant();

                if (!FontHeaders.ContainsKey(type) 
                    || !FontHeaders.Any(x => File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(x.Value)))
                {
                    Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Error);
                    return;
                }

                TextFontTask.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
        }

        private string CustomCursorArrowPath => Path.Combine(Paths.Modifications, @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png");
        private string CustomCursorFarPath => Path.Combine(Paths.Modifications, @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png");

        private string CustomCursorPath => CustomCursorArrowPath;

        private void ManageCustomCursor()
        {
            if (File.Exists(CustomCursorArrowPath))
            {
                File.Delete(CustomCursorArrowPath);
                File.Delete(CustomCursorFarPath);
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.Menu_IconFiles}|*.png;*.jpg;*.bmp"
                };

                if (dialog.ShowDialog() != true)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(CustomCursorArrowPath)!);

                ResizeAndSaveCursor(dialog.FileName, CustomCursorArrowPath, 64);
                ResizeAndSaveCursor(dialog.FileName, CustomCursorFarPath, 64);
            }

            OnPropertyChanged(nameof(ChooseCustomCursorVisibility));
            OnPropertyChanged(nameof(DeleteCustomCursorVisibility));
        }

        private void ResizeAndSaveCursor(string sourcePath, string destPath, int maxSize)
        {
            using var original = System.Drawing.Image.FromFile(sourcePath);

            int width, height;

            if (original.Width > original.Height)
            {
                width = maxSize;
                height = (int)(original.Height * ((float)maxSize / original.Width));
            }
            else
            {
                height = maxSize;
                width = (int)(original.Width * ((float)maxSize / original.Height));
            }

            using var resized = new System.Drawing.Bitmap(original, new System.Drawing.Size(width, height));
            resized.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public ICommand ManageCustomCursorCommand => new RelayCommand(ManageCustomCursor);

        public Visibility ChooseCustomCursorVisibility => !File.Exists(CustomCursorPath) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DeleteCustomCursorVisibility => !File.Exists(CustomCursorPath) ? Visibility.Collapsed : Visibility.Visible;

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        public Visibility ChooseCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DeleteCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

        public ICommand OpenCompatSettingsCommand => new RelayCommand(OpenCompatSettings);

        public ModPresetTask OldAvatarBackgroundTask { get; } = new("OldAvatarBackground", @"ExtraContent\places\Mobile.rbxl", "OldAvatarBackground.rbxl");

        public ModPresetTask OldCharacterSoundsTask { get; } = new("OldCharacterSounds", new()
        {
            { @"content\sounds\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3"  },
            { @"content\sounds\action_jump.mp3",              "Sounds.OldJump.mp3"  },
            { @"content\sounds\action_get_up.mp3",            "Sounds.OldGetUp.mp3" },
            { @"content\sounds\action_falling.mp3",           "Sounds.Empty.mp3"    },
            { @"content\sounds\action_jump_land.mp3",         "Sounds.Empty.mp3"    },
            { @"content\sounds\action_swim.mp3",              "Sounds.Empty.mp3"    },
            { @"content\sounds\impact_water.mp3",             "Sounds.Empty.mp3"    }
        });

        public EmojiModPresetTask EmojiFontTask { get; } = new();

        public EnumModPresetTask<Enums.CursorType> CursorTypeTask { get; } = new("CursorType", new()
        {
            {
                Enums.CursorType.From2006, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2006.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
                }
            },
            {
                Enums.CursorType.From2013, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2013.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
                }
            }
        });

        public FontModPresetTask TextFontTask { get; } = new();

        private void OpenCompatSettings()
        {
            string path = new RobloxPlayerData().ExecutablePath;

            if (File.Exists(path))
                PInvoke.SHObjectProperties(HWND.Null, SHOP_TYPE.SHOP_FILEPATH, path, "Compatibility");
            else
                Frontend.ShowMessageBox(Strings.Common_RobloxNotInstalled, MessageBoxImage.Error);

        }
    }
}

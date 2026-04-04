using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReactiveUI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace PNGBatchSaver.ViewModels;

public class MainViewModel : ViewModelBase
{
    private int _selectedCompressionLevel = 6;
    private string _statusText = "Drop PNG files here to re-save them with the selected compression level.";
    private bool _isProcessing = false;

    public List<int> CompressionLevels { get; } = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public int SelectedCompressionLevel
    {
        get => _selectedCompressionLevel;
        set => this.RaiseAndSetIfChanged(ref _selectedCompressionLevel, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        private set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
    }

    public async Task ProcessFilesAsync(IEnumerable<string> filePaths)
    {
        if (IsProcessing) return;
        IsProcessing = true;
        StatusText = "Processing...";

        int success = 0, failed = 0;
        var encoder = new PngEncoder
        {
            CompressionLevel = (PngCompressionLevel)SelectedCompressionLevel
        };

        foreach (var path in filePaths)
        {
            if (!File.Exists(path) || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                failed++;
                continue;
            }
            try
            {
                var dir = Path.GetDirectoryName(path) ?? Path.GetTempPath();
                var tmp = Path.Combine(dir, Path.GetRandomFileName() + ".tmp");
                try
                {
                    using (var image = await Image.LoadAsync(path))
                    {
                        await image.SaveAsPngAsync(tmp, encoder);
                    }
                    File.Move(tmp, path, overwrite: true);
                    success++;
                }
                catch
                {
                    if (File.Exists(tmp))
                        File.Delete(tmp);
                    throw;
                }
            }
            catch (IOException)
            {
                failed++;
            }
            catch (UnauthorizedAccessException)
            {
                failed++;
            }
            catch (ImageFormatException)
            {
                failed++;
            }
        }

        StatusText = $"Done. {success} file(s) saved with compression level {SelectedCompressionLevel}."
            + (failed > 0 ? $" {failed} file(s) failed or were skipped." : "");
        IsProcessing = false;
    }
}

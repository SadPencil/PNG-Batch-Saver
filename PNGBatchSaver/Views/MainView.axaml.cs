using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using PNGBatchSaver.ViewModels;

namespace PNGBatchSaver.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (!e.Data.Contains(DataFormats.Files))
            return;

        var files = e.Data.GetFiles();
        if (files == null)
            return;

        var paths = files
            .Select(f => f.Path.LocalPath)
            .Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (paths.Count > 0)
            await vm.ProcessFilesAsync(paths);
    }
}

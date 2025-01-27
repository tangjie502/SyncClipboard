﻿using Microsoft.Extensions.DependencyInjection;
using SyncClipboard.Abstract;
using SyncClipboard.Core.Models;
using SyncClipboard.Core.Utilities;

namespace SyncClipboard.Core.Clipboard;

public class ImageProfile : FileProfile
{
    public override ProfileType Type => ProfileType.Image;
    protected override IClipboardSetter<Profile> ClipboardSetter { get; set; }
    public override string FileName
    {
        get
        {
            if (string.IsNullOrEmpty(base.FileName))
            {
                FileName = Path.GetFileName(FullPath)!;
            }
            return base.FileName;
        }
        set => base.FileName = value;
    }

    public override string? FullPath
    {
        get
        {
            if (string.IsNullOrEmpty(base.FullPath) && Image is not null)
            {
                SaveImageToFile();
            }
            return base.FullPath;
        }
        set => base.FullPath = value;
    }

    private IClipboardImage? Image { get; set; }
    private readonly static string ImageTemplateFolder = Path.Combine(LocalTemplateFolder, "temp images");

    public ImageProfile(string filepath, IServiceProvider serviceProvider) : base(filepath, serviceProvider)
    {
        ClipboardSetter = serviceProvider.GetRequiredService<IClipboardSetter<ImageProfile>>();
    }

    public ImageProfile(ClipboardProfileDTO profileDTO, IServiceProvider serviceProvider) : base(profileDTO, serviceProvider)
    {
        ClipboardSetter = serviceProvider.GetRequiredService<IClipboardSetter<ImageProfile>>();
    }

    public ImageProfile(IClipboardImage image, IServiceProvider serviceProvider) : base("", serviceProvider)
    {
        ClipboardSetter = serviceProvider.GetRequiredService<IClipboardSetter<ImageProfile>>();
        Image = image;
    }

    private void SaveImageToFile()
    {
        ArgumentNullException.ThrowIfNull(Image);
        if (!Directory.Exists(ImageTemplateFolder))
        {
            Directory.CreateDirectory(ImageTemplateFolder);
        }
        var filePath = Path.Combine(ImageTemplateFolder, $"{Path.GetRandomFileName()}.png");
        Image.Save(filePath);
        FullPath = filePath;
    }

    protected override void SetNotification(INotification notification)
    {
        var path = FullPath ?? GetTempLocalFilePath();
        notification.SendImage(
            I18n.Strings.ClipboardImageUpdated,
            FileName,
            new Uri(path),
            DefaultButton(),
            new Button(I18n.Strings.OpenFolder, new Callbacker(Guid.NewGuid().ToString(), OpenInExplorer())),
            new Button(I18n.Strings.Open, new Callbacker(Guid.NewGuid().ToString(), (_) => Sys.OpenWithDefaultApp(path)))
        );
    }
}
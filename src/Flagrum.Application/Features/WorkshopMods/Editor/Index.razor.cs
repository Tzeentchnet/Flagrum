using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Components.Modals;
using Flagrum.Core.Archive;
using Flagrum.Core.Archive.Mod;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Features.WorkshopMods.Data;
using Flagrum.Application.Features.WorkshopMods.Services;
using Flagrum.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.WorkshopMods.Editor;

public partial class Index : ComponentBase
{
    [Parameter] public string NavigationParameter { get; set; }

    [Inject] private TextureConverter TextureConverter { get; set; }
    [Inject] private AppStateService AppState { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }
    [Inject] private IProfileService Profile { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private IPlatformService PlatformService { get; set; }
    [Inject] private ModelReplacementPresets ReplacementPresets { get; set; }
    [Inject] private BinmodBuilder BinmodBuilder { get; set; }
    [Inject] private Modmeta Modmeta { get; set; }
    [Inject] private IStringLocalizer<WorkshopMods.Index> ParentLocalizer { get; set; }

    public Binmod Mod { get; set; }
    public int StatsTotal { get; set; }
    private bool IsNew { get; set; }
    private bool CanSave { get; set; }
    private string LoadingText { get; set; }
    private bool IsLoading { get; set; }
    private string ImageName { get; set; } = "current_preview";
    private string ThumbnailName { get; set; } = "current_thumbnail";
    private Dictionary<int, string> ModTypes { get; set; }
    private Dictionary<int, string> ModTargets { get; set; }
    private WorkshopModBuildContext WorkshopModBuildContext { get; set; }
    private int ModelCount { get; set; } = 1;
    private Dictionary<int, string> ModelNames { get; set; }
    private bool[] HasSelectedDataForModel { get; } = new bool[2];
    private string[] FmdFileNames { get; } = new string[2];
    private PromptModal DeleteModal { get; set; }
    public string ModelReplacementPresetName { get; set; }

    protected override async Task OnInitializedAsync()
    {
        WorkshopModBuildContext = new WorkshopModBuildContext(TextureConverter, StateHasChanged);
        ModTypes = Enum.GetValues<WorkshopModType>().ToDictionary(t => (int)t, t => L[t.ToString()].Value);

        Mod = AppState.ActiveMod?.Clone();

        if (Mod == null)
        {
            await InitializeNewModAsync();
        }
        else
        {
            await InitializeExistingModAsync();
        }

        ModTargets = BinmodTypeHelper.GetTargets(Mod.Type)
            .ToDictionary(kvp => kvp.Key, kvp => ParentLocalizer[kvp.Value].Value);

        if (NavigationParameter != null)
        {
            if (NavigationParameter == "frompreset")
            {
                Mod.Type = (int)WorkshopModType.Character;
            }
        }
    }

    private void SetLoading(string text)
    {
        LoadingText = text;
        IsLoading = true;
        StateHasChanged();
    }

    private async Task InitializeNewModAsync()
    {
        IsNew = true;
        WorkshopModBuildContext.Flags |=
            WorkshopModBuildContextFlags.NeedsBuild | WorkshopModBuildContextFlags.PreviewImageChanged;

        var defaultPreviewPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png";
        var currentPreviewPath = $"{IOHelper.GetWebRoot()}\\images\\current_preview.png";
        File.Copy(defaultPreviewPath, currentPreviewPath, true);
        var previewBytes = File.ReadAllBytes(defaultPreviewPath);
        await WorkshopModBuildContext.ProcessPreviewImageAsync(previewBytes);

        Mod = new Binmod
        {
            Uuid = Guid.NewGuid().ToString(),
            IsApplyToGame = true
        };

        Mod.Path = $"{Profile.BinmodDirectory}\\{Mod.Uuid}.ffxvbinmod";
    }

    private async Task InitializeExistingModAsync()
    {
        ModelNames = BinmodTypeHelper.GetModelNames(Mod.Type, Mod.Target);
        ModelCount = BinmodTypeHelper.GetModelCount(Mod.Type, Mod.Target);

        if (ModelCount > 1)
        {
            Mod.Model1Name = ModelNames[0].ToSafeString();
            Mod.Model2Name = ModelNames[1].ToSafeString();
        }

        var previewBytes = Mod.GetPreviewPng();

        if (previewBytes.Length > 0)
        {
            await WorkshopModBuildContext.ProcessPreviewImageAsync(previewBytes);
            File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_preview.png", previewBytes);
        }
        else
        {
            var defaultPreviewPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png";
            var currentPreviewPath = $"{IOHelper.GetWebRoot()}\\images\\current_preview.png";
            File.Copy(defaultPreviewPath, currentPreviewPath, true);
            previewBytes = File.ReadAllBytes(defaultPreviewPath);
            await WorkshopModBuildContext.ProcessPreviewImageAsync(previewBytes);
        }

        if (Mod.Type == (int)WorkshopModType.StyleEdit)
        {
            if (Mod.HasThumbnailPng(out var thumbnailBytes))
            {
                await WorkshopModBuildContext.ProcessThumbnailImageAsync(thumbnailBytes);
                File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", thumbnailBytes);
            }
            else
            {
                try
                {
                    var jpgBytes = TextureConverter.ToJpeg(thumbnailBytes);
                    await WorkshopModBuildContext.ProcessThumbnailImageAsync(jpgBytes);
                    File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", jpgBytes);
                }
                catch
                {
                    // Must be a mod made with a previous version of Flagrum if the Btex conversion is failing
                    var defaultThumbnailPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\default.png";
                    var pngBytes = File.ReadAllBytes(defaultThumbnailPath);
                    await WorkshopModBuildContext.ProcessThumbnailImageAsync(pngBytes);
                    File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", pngBytes);
                }
            }
        }

        HasSelectedDataForModel[0] = true;
        HasSelectedDataForModel[1] = true;
        CanSave = true;
        StateHasChanged();
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/mod");
    }

    private async Task SelectImage()
    {
        await PlatformService.OpenFileDialogAsync(
            "Image Files|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif",
            async path =>
            {
                await WorkshopModBuildContext.ProcessPreviewImageAsync(path, async () =>
                {
                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ImageName = ImageName == "current_preview" ? "Current_Preview" : "current_preview";
                    await InvokeAsync(StateHasChanged);
                });
            });
    }

    private async Task SelectThumbnail()
    {
        await PlatformService.OpenFileDialogAsync(
            "Image Files|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif",
            async path =>
            {
                await WorkshopModBuildContext.ProcessThumbnailImageAsync(path, async () =>
                {
                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ThumbnailName = ThumbnailName == "current_thumbnail" ? "Current_Thumbnail" : "current_thumbnail";
                    await InvokeAsync(StateHasChanged);
                });
            });
    }

    private void Delete()
    {
        DeleteModal.Open();
    }

    private void OnDelete()
    {
        AppState.Mods.Remove(AppState.ActiveMod);
        AppState.UpdateBinmodList();
        File.Delete(Mod.Path);

        Navigation.NavigateTo("/mod");
    }

    private async Task Save()
    {
        if (Mod.Type == (int)WorkshopModType.Character)
        {
            Mod.GameMenuTitle = null;
        }

        File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\{Mod.Uuid}.png", WorkshopModBuildContext.PreviewImage);

        if (WorkshopModBuildContext.Flags.HasFlag(WorkshopModBuildContextFlags.NeedsBuild))
        {
            SetLoading(L["BuildingMod"]);
            await BuildAsync();
        }
        else
        {
            SetLoading(L["Saving"]);
            await SaveAsync();
        }
    }

    private async Task SaveAsync()
    {
        using var archive = new EbonyArchive(Mod.Path);

        var modmetaUri = archive.Files.First(f => f.Value.Uri.EndsWith("index.modmeta")).Value.Uri;
        archive.UpdateFile(modmetaUri, Modmeta.Build(Mod));

        if (WorkshopModBuildContext.Flags.HasFlag(WorkshopModBuildContextFlags.PreviewImageChanged))
        {
            await WorkshopModBuildContext.WaitForPreviewData(Mod.Type == (int)WorkshopModType.StyleEdit);
            var previewPngUri = archive.Files
                .First(f => f.Value.Uri.EndsWith("$preview.png")).Value.Uri;
            var previewBinUri = archive.Files
                .First(f => f.Value.Uri.EndsWith("$preview.png.bin")).Value.Uri;
            archive.UpdateFile(previewPngUri, WorkshopModBuildContext.PreviewBtex);
            archive.UpdateFile(previewBinUri, WorkshopModBuildContext.PreviewImage);

            if (Mod.Type == (int)WorkshopModType.StyleEdit)
            {
                var defaultPngUri = archive.Files
                    .First(f => f.Value.Uri.EndsWith("default.png")).Value.Uri;
                archive.UpdateFile(defaultPngUri, WorkshopModBuildContext.ThumbnailBtex);

                if (archive.HasFile($"data://mod/{Mod.ModDirectoryName}/default.png.bin"))
                {
                    archive.UpdateFile($"data://mod/{Mod.ModDirectoryName}/default.png.bin",
                        WorkshopModBuildContext.ThumbnailImage);
                }
                else
                {
                    archive.AddFile(WorkshopModBuildContext.ThumbnailImage,
                        $"data://mod/{Mod.ModDirectoryName}/default.png.bin");
                }
            }
        }

        archive.WriteToFile(Mod.Path, LuminousGame.FFXV);

        AppState.ActiveMod.UpdateFrom(Mod);
        Navigation.NavigateTo("/mod");
    }

    private async Task BuildAsync()
    {
        await WorkshopModBuildContext.WaitForBuildData(Mod.Type == (int)WorkshopModType.StyleEdit);

        Mod.ModelExtension = "gmdl";
        BinmodBuilder.Initialise(Mod, WorkshopModBuildContext);

        if (ModelCount > 1)
        {
            if (WorkshopModBuildContext.Fmds[0] == null)
            {
                BinmodBuilder.AddModelFromExisting(Mod, 0);
            }
            else
            {
                BinmodBuilder.AddFmd(0, WorkshopModBuildContext.Fmds[0]);
            }

            if (WorkshopModBuildContext.Fmds[1] == null)
            {
                BinmodBuilder.AddModelFromExisting(Mod, 1);
            }
            else
            {
                BinmodBuilder.AddFmd(1, WorkshopModBuildContext.Fmds[1]);
            }
        }
        else
        {
            BinmodBuilder.AddFmd(-1, WorkshopModBuildContext.Fmds[0]);
        }

        BinmodBuilder.WriteToFile(Mod.Path);

        if (AppState.ActiveMod == null)
        {
            AppState.Mods.Add(Mod);
        }
        else
        {
            AppState.ActiveMod.UpdateFrom(Mod);
        }

        AppState.UpdateBinmodList();
        Navigation.NavigateTo("/mod");
    }

    private void Upload()
    {
        Navigation.NavigateTo("/mod/upload");
    }

    private void ModTypeChanged(int newType)
    {
        ModTargets = BinmodTypeHelper.GetTargets(newType)
            .ToDictionary(kvp => kvp.Key, kvp => ParentLocalizer[kvp.Value].Value);
        Mod.Target = -1;
        StateHasChanged();
    }

    public void ModTargetChanged(int newTarget)
    {
        if (newTarget == -1)
        {
            return;
        }

        if (Mod.Type == (int)WorkshopModType.Character)
        {
            Mod.OriginalGmdls = ReplacementPresets.GetOriginalGmdls(newTarget).ToList();
        }

        ModelCount = BinmodTypeHelper.GetModelCount(Mod.Type, newTarget);
        ModelNames = BinmodTypeHelper.GetModelNames(Mod.Type, newTarget);

        if (ModelCount > 1)
        {
            Mod.Model1Name = ModelNames[0].ToSafeString();
            Mod.Model2Name = ModelNames[1].ToSafeString();
        }

        StateHasChanged();
    }

    private void VariantChanged(ChangeEventArgs e)
    {
        Mod.Gender = e.Value?.ToString();
    }

    private async Task SelectModel(int index)
    {
        await PlatformService.OpenFileDialogAsync(
            "Flagrum Model Data (*.fmd)|*.fmd",
            async path =>
            {
                FmdFileNames[index] = path.Split('\\', '/').Last();
                await WorkshopModBuildContext.ProcessFmdAsync(index, path);
                Mod.ModDirectoryName = Mod.Uuid;
                Mod.ModelName = path.Split('\\', '/').Last().Split('.')[0].ToSafeString();

                HasSelectedDataForModel[index] = true;

                if (ModelCount == 1)
                {
                    CanSave = true;
                }
                else
                {
                    CanSave = HasSelectedDataForModel[0] && HasSelectedDataForModel[1];
                }

                if (IsNew && CanSave && Mod.Type == (int)WorkshopModType.StyleEdit)
                {
                    var defaultThumbnailPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\default.png";
                    var currentThumbnailPath = $"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png";
                    File.Copy(defaultThumbnailPath, currentThumbnailPath, true);
                    var thumbnailBytes = await File.ReadAllBytesAsync(defaultThumbnailPath);
                    await WorkshopModBuildContext.ProcessThumbnailImageAsync(thumbnailBytes);

                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ThumbnailName = ThumbnailName == "current_thumbnail" ? "Current_Thumbnail" : "current_thumbnail";
                }

                await InvokeAsync(StateHasChanged);
            });
    }
}
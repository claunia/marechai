#nullable enable

using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Views;

namespace Marechai.App.Navigation;

/// <summary>
///     Maps a <see cref="SearchResultDto" /> (entity type values match
///     <c>Marechai.Data.SearchEntityType</c>) to its detail page and navigates there.
/// </summary>
public static class SearchResultNavigator
{
    public static void NavigateTo(IRegionManager regionManager, SearchResultDto result)
    {
        if(result.EntityType is null || result.EntityId is null) return;

        long entityId = result.EntityId.Value;

        switch(result.EntityType.Value)
        {
            case 1: // Company
                Navigate(regionManager, nameof(CompanyDetailPage), NavParamKeys.CompanyId, (int)entityId);

                break;
            case 2: // Computer
            case 3: // Console
            case 4: // Smartphone
            case 14: // Pda
            case 15: // Tablet
                Navigate(regionManager, nameof(MachineViewPage), NavParamKeys.MachineId, (int)entityId);

                break;
            case 5: // Book
                Navigate(regionManager, nameof(BookViewPage), NavParamKeys.BookId, entityId);

                break;
            case 6: // Document
                Navigate(regionManager, nameof(DocumentViewPage), NavParamKeys.DocumentId, entityId);

                break;
            case 7: // Magazine
                Navigate(regionManager, nameof(MagazineViewPage), NavParamKeys.MagazineId, entityId);

                break;
            case 8: // Gpu
                Navigate(regionManager, nameof(GpuDetailPage), NavParamKeys.GpuId, (int)entityId);

                break;
            case 9: // Processor
                Navigate(regionManager, nameof(ProcessorDetailPage), NavParamKeys.ProcessorId, (int)entityId);

                break;
            case 10: // SoundSynth
                Navigate(regionManager, nameof(SoundSynthDetailPage), NavParamKeys.SoundSynthId, (int)entityId);

                break;
            case 11: // Person
                Navigate(regionManager, nameof(PersonViewPage), NavParamKeys.PersonId, (int)entityId);

                break;
            case 12: // Software
                Navigate(regionManager, nameof(SoftwareViewPage), NavParamKeys.SoftwareId, (int)entityId);

                break;
            case 13: // SoftwareCompilation
                Navigate(regionManager, nameof(SoftwareReleaseViewPage), NavParamKeys.SoftwareReleaseId,
                        (int)entityId);

                break;
        }
    }

    private static void Navigate(IRegionManager regionManager, string pageName, string paramKey, int id)
    {
        var parameters = new NavigationParameters { { paramKey, id }, { NavParamKeys.NavigationSource, "Search" } };

        regionManager.RequestNavigate(RegionNames.Content, pageName, parameters);
    }

    private static void Navigate(IRegionManager regionManager, string pageName, string paramKey, long id)
    {
        var parameters = new NavigationParameters { { paramKey, id }, { NavParamKeys.NavigationSource, "Search" } };

        regionManager.RequestNavigate(RegionNames.Content, pageName, parameters);
    }
}

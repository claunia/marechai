using Prism.Navigation.Regions;

namespace Marechai.App.Navigation;

public static class RegionNavigationExtensions
{
    public static bool TryGoBack(this IRegionManager regionManager, string regionName)
    {
        if(!regionManager.Regions.ContainsRegionWithName(regionName))
            return false;

        IRegionNavigationJournal journal = regionManager.Regions[regionName].NavigationService.Journal;

        if(!journal.CanGoBack)
            return false;

        journal.GoBack();

        return true;
    }
}

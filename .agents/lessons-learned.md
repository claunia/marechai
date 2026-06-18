# Lessons Learned

## 2026-06-18

- In `Marechai/Pages/Admin/SoftwareReleaseDialog.razor`, release regions and languages cannot rely on `ReleaseId` during add mode because the release does not exist yet. Keep provisional junction selections in local dialog state and return them through `SoftwareReleaseDialogResult`.
- Persist add-mode release regions and languages only after `SoftwareReleasesService.CreateAsync(...)` succeeds in `Marechai/Pages/Admin/SoftwareReleases.razor.cs`.
- When fixing add-mode behavior for one release junction picker in this dialog, check the sibling pickers for the same `ReleaseId <= 0` pattern. Regions and languages had the same bug shape.

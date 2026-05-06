using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class UserReviewDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public int SoftwareId { get; set; }

    [Parameter]
    public SoftwareUserReviewDto ExistingReview { get; set; }

    [Parameter]
    public float CurrentRating { get; set; }

    [Inject]
    SoftwareService SoftwareService { get; set; }

    [Inject]
    ISnackbar Snackbar { get; set; }

    string _theGood;
    string _theBad;
    string _theUgly;
    bool   _isAnonymous;
    float  _ratingValue;

    protected override void OnInitialized()
    {
        _ratingValue = CurrentRating;

        if(ExistingReview is not null)
        {
            _theGood     = ExistingReview.TheGood;
            _theBad      = ExistingReview.TheBad;
            _theUgly     = ExistingReview.TheUgly;
            _isAnonymous = ExistingReview.IsAnonymous.GetValueOrDefault();
        }
    }

    void Cancel() => MudDialog.Cancel();

    async Task SubmitAsync()
    {
        var dto = new SoftwareUserReviewDto
        {
            TheGood     = _theGood,
            TheBad      = _theBad,
            TheUgly     = _theUgly,
            IsAnonymous = _isAnonymous,
            Rating      = _ratingValue > 0 ? _ratingValue : null
        };

        if(ExistingReview is not null)
        {
            (bool succeeded, string error) =
                await SoftwareService.UpdateUserReviewAsync(SoftwareId, ExistingReview.Id.GetValueOrDefault(), dto);

            if(succeeded)
            {
                Snackbar.Add("Review updated successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add(error ?? "Error", Severity.Error);
            }
        }
        else
        {
            (SoftwareUserReviewDto result, string error) =
                await SoftwareService.CreateUserReviewAsync(SoftwareId, dto);

            if(result is not null)
            {
                Snackbar.Add("Review submitted successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add(error ?? "Error", Severity.Error);
            }
        }
    }
}

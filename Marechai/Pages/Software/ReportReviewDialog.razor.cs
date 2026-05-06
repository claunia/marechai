using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class ReportReviewDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public int SoftwareId { get; set; }

    [Parameter]
    public long ReviewId { get; set; }

    [Inject]
    SoftwareService SoftwareService { get; set; }

    [Inject]
    ISnackbar Snackbar { get; set; }

    ReviewReportReason _reason;
    string             _explanation;

    void Cancel() => MudDialog.Cancel();

    async Task SubmitAsync()
    {
        var dto = new CreateReviewReportRequest
        {
            Reason      = (int?)_reason,
            Explanation = _explanation
        };

        (bool succeeded, string error) = await SoftwareService.ReportReviewAsync(SoftwareId, ReviewId, dto);

        if(succeeded)
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            Snackbar.Add(error ?? "Error", Severity.Error);
        }
    }
}

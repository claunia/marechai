namespace Marechai.Pages.Admin;

public sealed class UserDialogResult
{
    public string  Email       { get; set; } = null!;
    public string  UserName    { get; set; } = null!;
    public string PhoneNumber { get; set; }
    public string Password    { get; set; }
}

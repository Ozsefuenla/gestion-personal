using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GestionPersonal.Web.Components;

public partial class CenterLoginDialog
{
    [CascadingParameter]
    private IMudDialogInstance DialogInstance { get; set; } = default!;

    private Task OnLoggedInAsync()
    {
        DialogInstance.Close(DialogResult.Ok(true));
        return Task.CompletedTask;
    }

    private void Cancel() => DialogInstance.Cancel();
}

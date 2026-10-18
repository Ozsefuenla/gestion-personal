using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GestionPersonal.Web.Components;

public partial class CenterLoginForm
{
    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Parameter]
    public EventCallback OnLoggedInAsync { get; set; }

    private MudForm _form = default!;
    private MudTextField<string> _usernameField = default!;
    private string _username = "";
    private string _password = "";
    private string? _error;
    private bool _loading;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(200);
            await _usernameField.FocusAsync();
        }
    }

    private async Task SubmitAsync()
    {
        _error = null;
        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            return;
        }

        _loading = true;

        try
        {
            var result = await Js.InvokeAsync<BrowserLoginResult>("centerLogin", _username.Trim(), _password);

            if (!result.Ok)
            {
                _error = result.Error;
                return;
            }

            if (result.MustChangePassword && !await ForceChangePasswordAsync())
            {
                return;
            }

            await OnLoggedInAsync.InvokeAsync();
        }
        catch (Exception)
        {
            _error = "No se pudo conectar. Inténtalo de nuevo.";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task<bool> ForceChangePasswordAsync()
    {
        var parameters = new DialogParameters { ["CurrentPassword"] = _password };
        var options = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, CloseButton = false };

        var dialog = await DialogService.ShowAsync<ChangePasswordDialog>("Debes cambiar tu contraseña", parameters, options);
        var result = await dialog.Result;

        return result is not null && !result.Canceled;
    }

    private void ForgotPassword() =>
        Snackbar.Add("Para recuperar tu contraseña, ponte en contacto con soporte.", Severity.Info);

    private sealed record BrowserLoginResult(bool Ok, string? Error, bool MustChangePassword);
}

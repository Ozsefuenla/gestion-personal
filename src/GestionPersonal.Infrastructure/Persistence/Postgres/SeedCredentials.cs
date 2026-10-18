namespace GestionPersonal.Infrastructure.Persistence.Postgres;

public sealed record SeedCredentials(
    string SuperAdminUsername,
    string SuperAdminPassword,
    string CenterAdminUsername,
    string CenterAdminPassword,
    string CenterUsername,
    string CenterPassword)
{
    public static SeedCredentials FromConfiguration(
        Func<string, string?> getValue) =>
        new(
            getValue("Persistence:SuperAdminUsername") ?? "superadmin",
            getValue("Persistence:SuperAdminPassword") ?? "superadmin",
            getValue("Persistence:CenterAdminUsername") ?? "carlos.ruiz",
            getValue("Persistence:CenterAdminPassword") ?? "sol",
            getValue("Persistence:CenterUsername") ?? "centro",
            getValue("Persistence:CenterPassword") ?? "centro");
}

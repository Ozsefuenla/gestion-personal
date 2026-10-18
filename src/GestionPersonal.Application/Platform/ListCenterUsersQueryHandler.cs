using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Platform;

public sealed class ListCenterUsersQueryHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly ICurrentUser _currentUser;

    public ListCenterUsersQueryHandler(ICenterUserRepository centerUserRepository, ICurrentUser currentUser)
    {
        _centerUserRepository = centerUserRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CenterUserResponse>>> Handle(
        ListCenterUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.Role == UserRole.SuperAdmin)
        {
            var users = await _centerUserRepository.GetByCenterAsync(query.CenterId, cancellationToken);

            IReadOnlyList<CenterUserResponse> superResponse = users
                .Where(u => u.Role is UserRole.CenterAdmin or UserRole.Center)
                .Select(CenterUserResponse.From)
                .ToList();

            return Result<IReadOnlyList<CenterUserResponse>>.Success(superResponse);
        }

        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        if (query.CenterId != _currentUser.CenterId!.Value)
        {
            return Error.Forbidden("No autorizado para este centro.");
        }

        var centerUsers = await _centerUserRepository.GetByCenterAsync(query.CenterId, cancellationToken);
        var visible = centerUsers
            .Where(u => u.Role == UserRole.Center)
            .Select(CenterUserResponse.From)
            .ToList();

        return Result<IReadOnlyList<CenterUserResponse>>.Success(visible);
    }
}

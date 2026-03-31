using MediatR;

namespace AILifeAnalytics.Application.Queries.Auth;

/// <summary>
/// Запрос данных текущего авторизованного пользователя
/// </summary>
public record GetCurrentUserQuery(Guid UserId, string Email, string Name, string Role) : IRequest<object>;

/// <summary>
/// Обработчик запроса данных текущего пользователя
/// </summary>
public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, object>
{
    public Task<object> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        => Task.FromResult<object>(new
        {
            id = query.UserId,
            email = query.Email,
            name = query.Name,
            role = query.Role
        });
}
using OpenPlanningPoker.UserManagement.Api.Enums;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace OpenPlanningPoker.UserManagement.Api.Features.Users;

public static class RegisterUser
{
    public sealed record RegisterUserCommand(string Username, string Email, string FirstName, string LastName, string Password);

    public sealed class RegisterUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterUserCommand, UserRepresentation>()
                .ForMember(s => s.EmailVerified, o => o.MapFrom(_ => true))
                .ForMember(s => s.Enabled, o => o.MapFrom(_ => true))
                .ForMember(s => s.Credentials, o => o.MapFrom(d => new List<CredentialRepresentation>
                {
                    new () { Type = $"{CredentialsTypes.Password}".ToLower(), Value = d.Password }
                }));
            CreateMap<UserRepresentation, RegisterUserResponse>();
        }
    }

    public class Validator : AbstractValidator<RegisterUserCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Username)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(16);

            RuleFor(c => c.FirstName)
                .NotEmpty()
                .MaximumLength(16);

            RuleFor(c => c.LastName)
                .NotEmpty()
                .MaximumLength(16);

            RuleFor(c => c.Password)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(26);

            RuleFor(c => c.Email)
                .EmailAddress()
                .NotEmpty();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("users/register", async (
                [FromServices] IKeycloakUserClient keyCloakUserClient,
                [FromServices] KeycloakProtectionClientOptions keycloakClientOptions,
                [FromServices] IMapper mapper,
                RegisterUserCommand command,
                CancellationToken cancellationToken) =>
                {
                    var mappedUser = mapper.Map<UserRepresentation>(command);
                    await keyCloakUserClient.CreateUserAsync(keycloakClientOptions.Realm, mappedUser, cancellationToken);

                    var userResponse = (await keyCloakUserClient.GetUsersAsync(keycloakClientOptions.Realm, new GetUsersRequestParameters()
                    {
                        Username = command.Username,
                        Email = command.Email
                    }, cancellationToken)).First();

                    return mapper.Map<RegisterUserResponse>(userResponse);
                })
                .WithTags("Users")
                .RequireAuthorization()
                .AddFluentValidationAutoValidation();
        }
    }
}
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Glense.AccountService.Data;
using Glense.AccountService.Protos;

namespace Glense.AccountService.GrpcServices
{
    public class AccountGrpcService : AccountGrpc.AccountGrpcBase
    {
        private readonly AccountDbContext _context;
        private readonly ILogger<AccountGrpcService> _logger;

        public AccountGrpcService(AccountDbContext context, ILogger<AccountGrpcService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<GetUsernameResponse> GetUsername(
            GetUsernameRequest request, ServerCallContext context)
        {
            _logger.LogDebug("gRPC GetUsername called for UserId={UserId}", request.UserId);

            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return new GetUsernameResponse { UserId = request.UserId, Username = "", Found = false };
            }

            var user = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .Select(u => new { u.Username })
                .FirstOrDefaultAsync(context.CancellationToken);

            if (user == null)
            {
                return new GetUsernameResponse { UserId = request.UserId, Username = "", Found = false };
            }

            return new GetUsernameResponse
            {
                UserId = request.UserId,
                Username = user.Username,
                Found = true
            };
        }

        public override async Task<GetUsernamesResponse> GetUsernames(
            GetUsernamesRequest request, ServerCallContext context)
        {
            _logger.LogDebug("gRPC GetUsernames called for {Count} user IDs", request.UserIds.Count);

            var response = new GetUsernamesResponse();

            var guids = request.UserIds
                .Select(id => Guid.TryParse(id, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .Distinct()
                .ToList();

            if (guids.Count == 0)
                return response;

            var users = await _context.Users
                .Where(u => guids.Contains(u.Id) && u.IsActive)
                .Select(u => new { u.Id, u.Username })
                .ToListAsync(context.CancellationToken);

            foreach (var user in users)
            {
                response.Users.Add(new UserMapping
                {
                    UserId = user.Id.ToString(),
                    Username = user.Username
                });
            }

            return response;
        }
    }
}

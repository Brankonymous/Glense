using Glense.VideoCatalogue.Protos;

namespace Glense.VideoCatalogue.GrpcClients
{
    public interface IAccountGrpcClient
    {
        Task<string?> GetUsernameAsync(Guid userId);
        Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds);
    }

    public class AccountGrpcClient : IAccountGrpcClient
    {
        private readonly AccountGrpc.AccountGrpcClient _client;
        private readonly ILogger<AccountGrpcClient> _logger;

        public AccountGrpcClient(
            AccountGrpc.AccountGrpcClient client,
            ILogger<AccountGrpcClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<string?> GetUsernameAsync(Guid userId)
        {
            try
            {
                var response = await _client.GetUsernameAsync(new GetUsernameRequest
                {
                    UserId = userId.ToString()
                });

                return response.Found ? response.Username : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "gRPC call to GetUsername failed for UserId={UserId}", userId);
                return null;
            }
        }

        public async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds)
        {
            var result = new Dictionary<Guid, string>();
            var uniqueIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();

            if (uniqueIds.Count == 0)
                return result;

            try
            {
                var request = new GetUsernamesRequest();
                request.UserIds.AddRange(uniqueIds.Select(id => id.ToString()));

                var response = await _client.GetUsernamesAsync(request);

                foreach (var mapping in response.Users)
                {
                    if (Guid.TryParse(mapping.UserId, out var id))
                    {
                        result[id] = mapping.Username;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "gRPC call to GetUsernames failed for {Count} user IDs", uniqueIds.Count);
            }

            return result;
        }
    }
}

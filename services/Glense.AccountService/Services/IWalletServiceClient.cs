namespace Glense.AccountService.Services
{
    public interface IWalletServiceClient
    {
        Task<bool> CreateWalletAsync(Guid userId, decimal initialBalance = 0);
    }
}

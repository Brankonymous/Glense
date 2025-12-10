using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;

namespace DonationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly DonationDbContext _context;

    public WalletController(DonationDbContext context)
    {
        _context = context;
    }

    /// Get wallet for a user
    [HttpGet("{userId}")]
    public async Task<ActionResult<Wallet>> GetWallet(int userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return NotFound($"Wallet for user {userId} not found");
        }

        return Ok(wallet);
    }

    /// Get all wallets
    [HttpGet]
    public async Task<ActionResult<List<Wallet>>> GetAllWallets()
    {
        var wallets = await _context.Wallets.ToListAsync();
        return Ok(wallets);
    }
}

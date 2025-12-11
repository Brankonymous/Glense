using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;
using DonationService.DTOs;

namespace DonationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WalletController : ControllerBase
{
    private readonly DonationDbContext _context;
    private readonly ILogger<WalletController> _logger;

    public WalletController(DonationDbContext context, ILogger<WalletController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get wallet by user ID
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletResponse>> GetWalletByUserId(int userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return NotFound(new { message = "Wallet not found" });
        }

        return Ok(new WalletResponse(
            wallet.Id, wallet.UserId, wallet.Balance,
            wallet.CreatedAt, wallet.UpdatedAt));
    }

    /// <summary>
    /// Create a new wallet for a user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WalletResponse>> CreateWallet([FromBody] CreateWalletRequest request)
    {
        // Check if wallet already exists for this user
        var existingWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == request.UserId);

        if (existingWallet != null)
        {
            return Conflict(new { message = "Wallet already exists for this user" });
        }

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = request.InitialBalance,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Wallet created for user {UserId} with initial balance {Balance}",
            request.UserId, request.InitialBalance);

        var response = new WalletResponse(
            wallet.Id, wallet.UserId, wallet.Balance,
            wallet.CreatedAt, wallet.UpdatedAt);

        return Created($"/api/wallet/user/{wallet.UserId}", response);
    }

    /// <summary>
    /// Add funds to a wallet (top-up)
    /// </summary>
    [HttpPost("user/{userId:int}/topup")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletResponse>> TopUpWallet(int userId, [FromBody] TopUpWalletRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero" });
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet == null)
        {
            return NotFound(new { message = "Wallet not found" });
        }

        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Wallet topped up for user {UserId}, amount: {Amount}, new balance: {Balance}",
            userId, request.Amount, wallet.Balance);

        return Ok(new WalletResponse(
            wallet.Id, wallet.UserId, wallet.Balance,
            wallet.CreatedAt, wallet.UpdatedAt));
    }

}


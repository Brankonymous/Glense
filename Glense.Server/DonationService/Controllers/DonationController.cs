using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;
using DonationService.DTOs;

namespace DonationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DonationController : ControllerBase
{
    private readonly DonationDbContext _context;
    private readonly ILogger<DonationController> _logger;

    public DonationController(DonationDbContext context, ILogger<DonationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all donations made by a specific user
    /// </summary>
    [HttpGet("donor/{userId:int}")]
    [ProducesResponseType(typeof(List<DonationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DonationResponse>>> GetDonationsByDonor(int userId)
    {
        var donations = await _context.Donations
            .Where(d => d.DonorUserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonationResponse(
                d.Id, d.DonorUserId, d.RecipientUserId,
                d.Amount, d.Message, d.CreatedAt))
            .ToListAsync();

        return Ok(donations);
    }

    /// <summary>
    /// Get all donations received by a specific user
    /// </summary>
    [HttpGet("recipient/{userId:int}")]
    [ProducesResponseType(typeof(List<DonationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DonationResponse>>> GetDonationsByRecipient(int userId)
    {
        var donations = await _context.Donations
            .Where(d => d.RecipientUserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DonationResponse(
                d.Id, d.DonorUserId, d.RecipientUserId,
                d.Amount, d.Message, d.CreatedAt))
            .ToListAsync();

        return Ok(donations);
    }

    /// <summary>
    /// Create a new donation (transfers funds between wallets)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DonationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DonationResponse>> CreateDonation([FromBody] CreateDonationRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero" });
        }

        if (request.DonorUserId == request.RecipientUserId)
        {
            return BadRequest(new { message = "Cannot donate to yourself" });
        }

        // Check if we can use transactions (not supported by in-memory database)
        var supportsTransactions = !_context.Database.IsInMemory();
        var transaction = supportsTransactions
            ? await _context.Database.BeginTransactionAsync()
            : null;

        try
        {
            // Get donor's wallet
            var donorWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.DonorUserId);

            if (donorWallet == null)
            {
                return NotFound(new { message = "Donor wallet not found" });
            }

            if (donorWallet.Balance < request.Amount)
            {
                return BadRequest(new { message = "Insufficient funds" });
            }

            // Get or create recipient's wallet
            var recipientWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.RecipientUserId);

            if (recipientWallet == null)
            {
                recipientWallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = request.RecipientUserId,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(recipientWallet);
            }

            // Perform the transfer
            donorWallet.Balance -= request.Amount;
            donorWallet.UpdatedAt = DateTime.UtcNow;

            recipientWallet.Balance += request.Amount;
            recipientWallet.UpdatedAt = DateTime.UtcNow;

            // Create donation record
            var donation = new Donation
            {
                Id = Guid.NewGuid(),
                DonorUserId = request.DonorUserId,
                RecipientUserId = request.RecipientUserId,
                Amount = request.Amount,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            _logger.LogInformation(
                "Donation created: {DonationId}, from user {DonorId} to user {RecipientId}, amount: {Amount}",
                donation.Id, request.DonorUserId, request.RecipientUserId, request.Amount);

            var response = new DonationResponse(
                donation.Id, donation.DonorUserId, donation.RecipientUserId,
                donation.Amount, donation.Message, donation.CreatedAt);

            return Created($"/api/donation/{donation.Id}", response);
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            _logger.LogError(ex, "Error creating donation");
            return StatusCode(500, new { message = "An error occurred while processing the donation" });
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}

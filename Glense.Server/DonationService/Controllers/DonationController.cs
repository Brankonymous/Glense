using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DonationService.Data;
using DonationService.Entities;

namespace DonationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationController : ControllerBase
{
    private readonly DonationDbContext _context;

    public DonationController(DonationDbContext context)
    {
        _context = context;
    }

    /// Get a specific donation by ID
    [HttpGet("{donationId:guid}")]
    public async Task<ActionResult<Donation>> GetDonation(Guid donationId)
    {
        var donation = await _context.Donations.FindAsync(donationId);

        if (donation == null)
        {
            return NotFound("Donation not found");
        }

        return Ok(donation);
    }

    /// Get all donations
    [HttpGet]
    public async Task<ActionResult<List<Donation>>> GetAllDonations()
    {
        var donations = await _context.Donations.ToListAsync();
        return Ok(donations);
    }
}

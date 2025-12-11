namespace DonationService.DTOs;

/// <summary>
/// Request DTO for creating a new donation
/// </summary>
public record CreateDonationRequest(
    int DonorUserId,
    int RecipientUserId,
    decimal Amount,
    string? Message
);

/// <summary>
/// Response DTO for donation data
/// </summary>
public record DonationResponse(
    Guid Id,
    int DonorUserId,
    int RecipientUserId,
    decimal Amount,
    string? Message,
    DateTime CreatedAt
);

/// <summary>
/// Request DTO for creating/topping up a wallet
/// </summary>
public record CreateWalletRequest(
    int UserId,
    decimal InitialBalance = 0
);

/// <summary>
/// Request DTO for adding funds to wallet
/// </summary>
public record TopUpWalletRequest(
    decimal Amount
);

/// <summary>
/// Response DTO for wallet data
/// </summary>
public record WalletResponse(
    Guid Id,
    int UserId,
    decimal Balance,
    DateTime CreatedAt,
    DateTime UpdatedAt
);


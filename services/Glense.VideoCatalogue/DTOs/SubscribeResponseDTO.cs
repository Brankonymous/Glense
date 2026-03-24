using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class SubscribeResponseDTO
{
    [Required]
    public Guid SubscriberId { get; set; }
    [Required]
    public Guid SubscribedToId { get; set; }
    public DateTime SubscriptionDate { get; set; }
}

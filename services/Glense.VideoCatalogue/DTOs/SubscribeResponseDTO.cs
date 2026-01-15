using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class SubscribeResponseDTO
{
    [Required]
    public int SubscriberId { get; set; }
    [Required]
    public int SubscribedToId { get; set; }
    public DateTime SubscriptionDate { get; set; }
}

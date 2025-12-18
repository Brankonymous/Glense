using System;

namespace Glense.VideoCatalogue.DTOs;

public class SubscribeResponseDTO
{
    public int SubscriberId { get; set; }
    public int SubscribedToId { get; set; }
    public DateTime SubscriptionDate { get; set; }
}

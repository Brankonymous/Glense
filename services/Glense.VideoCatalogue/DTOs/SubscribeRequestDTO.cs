using System;
using System.ComponentModel.DataAnnotations;

namespace Glense.VideoCatalogue.DTOs;

public class SubscribeRequestDTO
{
    [Required]
    public Guid SubscribedToId { get; set; }
}

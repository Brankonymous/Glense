using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("Subscriptions")]
public class Subscriptions
{
    [Column("subscriber_id")]
    public Guid SubscriberId { get; set; }

    [Column("subscribed_to_id")]
    public Guid SubscribedToId { get; set; }

    [Column("subscription_date")]
    public DateTime SubscriptionDate { get; set; }
}



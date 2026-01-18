using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glense.VideoCatalogue.Models;

[Table("Subscriptions")]
public class Subscriptions
{
    [Column("subscriber_id")]
    public int SubscriberId { get; set; }

    [Column("subscribed_to_id")]
    public int SubscribedToId { get; set; }

    [Column("subscription_date")]
    public DateTime SubscriptionDate { get; set; }
}



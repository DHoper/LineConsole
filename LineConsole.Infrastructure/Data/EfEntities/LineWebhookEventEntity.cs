using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LineConsole.Domain.Entities;

namespace LineConsole.Infrastructure.Data.EfEntities;

/// <summary>Webhook 收件匣（同一個帳號的 webhook_event_id 唯一：去重）</summary>
[Table("line_webhook_events")]
public class LineWebhookEventEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("account_id")]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("webhook_event_id")]
    public string WebhookEventId { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(64)]
    [Column("line_user_id")]
    public string? LineUserId { get; set; }

    [Required]
    [Column("occurred_at")]
    public DateTime OccurredAt { get; set; }

    [Required]
    [Column("is_redelivery")]
    public bool IsRedelivery { get; set; }

    [Required]
    [Column("payload")]
    public string Payload { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    public LineWebhookEventStatus Status { get; set; }

    [Required]
    [Column("received_at")]
    public DateTime ReceivedAt { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [MaxLength(500)]
    [Column("error")]
    public string? Error { get; set; }
}

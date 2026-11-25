using LineConsole.Domain.Entities;

namespace LineConsole.Tests.Webhooks;

/// <summary>好友狀態：LINE 不保證事件的到達順序，所以每個狀態變化都比較「事件發生時間」，舊的晚到就忽略</summary>
public class LineFriendTests
{
    private static readonly DateTime T0 = new(2026, 11, 20, 2, 0, 0, DateTimeKind.Utc);

    private static LineFriend NewFriend(DateTime at)
    {
        var f = LineFriend.FirstSeen(Guid.NewGuid(), "U" + new string('a', 32), at, at);
        Assert.True(f.Follow(at));
        return f;
    }

    [Fact]
    public void Block_then_follow_again_is_a_refollow()
    {
        var f = NewFriend(T0);
        Assert.True(f.Block(T0.AddHours(1)));
        Assert.Equal(LineFriendStatus.Blocked, f.Status);
        Assert.Equal(T0.AddHours(1), f.BlockedAt);

        Assert.True(f.Follow(T0.AddHours(2)));
        Assert.Equal(LineFriendStatus.Following, f.Status);
        Assert.Null(f.BlockedAt);
        Assert.Equal(T0.AddHours(2), f.FollowedAt);
    }

    [Fact]
    public void Older_event_arriving_late_is_ignored()
    {
        var f = NewFriend(T0);
        Assert.True(f.Block(T0.AddHours(2)));

        Assert.False(f.Follow(T0.AddHours(1)));   // 一小時前的 follow 現在才到
        Assert.Equal(LineFriendStatus.Blocked, f.Status);
    }

    [Fact]
    public void Follow_while_already_following_is_not_a_change()
    {
        var f = NewFriend(T0);
        Assert.False(f.Follow(T0.AddHours(1)));
        Assert.Equal(T0, f.FollowedAt);           // 加入時間不會被洗掉
    }

    [Fact]
    public void Older_message_does_not_move_last_message_back()
    {
        var f = NewFriend(T0);
        f.Message(T0.AddHours(3));
        f.Message(T0.AddHours(1));
        Assert.Equal(T0.AddHours(3), f.LastMessageAt);
    }
}

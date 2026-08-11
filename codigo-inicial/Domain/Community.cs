public sealed class Community
{
    public int Id { get; set; }
    public CommunityName Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
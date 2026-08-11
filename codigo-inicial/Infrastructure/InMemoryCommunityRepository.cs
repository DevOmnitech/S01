public sealed class InMemoryCommunityRepository : ICommunityRepository
{
    private readonly List<Community> _items = new();
    private int _nextId = 1;

    public Community Add(Community community)
    {
        community.Id = _nextId++;
        _items.Add(community);
        return community;
    }
    public Community? GetById(int id)
    {
        return _items.FirstOrDefault(x => x.Id == id);
    }
    public Community Rename(Community community, CommunityName newName)
    {
        var existing = GetById(community.Id);
        if (existing == null)
            throw new InvalidOperationException("Community not found");

        _items.Remove(existing);
        community.Name = newName;
        _items.Add(community);
        return community;
    }
}
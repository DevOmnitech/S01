public interface ICommunityRepository
{
    Community Add(Community community);
    Community? GetById(int id);

    Community Rename(Community community, CommunityName newName);

}
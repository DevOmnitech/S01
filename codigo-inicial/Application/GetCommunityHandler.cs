public sealed class GetCommunityHandler
{
    private readonly ICommunityRepository _repository;
    private readonly ILogger<GetCommunityHandler> _logger;

    public GetCommunityHandler(ICommunityRepository repository,
                               ILogger<GetCommunityHandler> logger)
        => (_repository, _logger) = (repository, logger);

    public Result<Community?> Handle(int id)
    {
        var community = _repository.GetById(id);
        if (community == null)
            return Result.Fail<Community?>("Comunidad no encontrada");

        _logger.LogInformation("Comunidad obtenida {CommunityId} {CommunityName}", community.Id, community.Name.Value);

        return Result<Community?>.Ok(community);
    }
}
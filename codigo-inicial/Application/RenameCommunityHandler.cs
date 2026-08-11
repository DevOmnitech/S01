public sealed class RenameCommunityHandler
{
    private readonly ICommunityRepository _repository;
    private readonly ILogger<RenameCommunityHandler> _logger;

    public RenameCommunityHandler(ICommunityRepository repository,
                                  ILogger<RenameCommunityHandler> logger)
        => (_repository, _logger) = (repository, logger);

    public Result<Community> Handle(int id, CommunityName newName)
    {
        var community = _repository.GetById(id);
        if (community == null)
            return Result.Fail<Community>("Comunidad no encontrada");

        var oldName = community.Name.Value;
        var renamed = _repository.Rename(community, newName);

        _logger.LogInformation("Comunidad renombrada {CommunityId} {OldName} -> {NewName}", renamed.Id, oldName, renamed.Name.Value);

        return Result.Ok(renamed);
    }
}
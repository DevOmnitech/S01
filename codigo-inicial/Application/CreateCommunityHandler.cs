public sealed class CreateCommunityHandler
{
    private readonly ICommunityRepository _repository;
    private readonly ILogger<CreateCommunityHandler> _logger;

    public CreateCommunityHandler(ICommunityRepository repository,
                                  ILogger<CreateCommunityHandler> logger)
        => (_repository, _logger) = (repository, logger);

    public Result<Community> Handle(string? rawName)
    {
        var nameResult = CommunityName.Create(rawName);
        if (!nameResult.IsSuccess)
            return Result.Fail<Community>(nameResult.Error!);

        var saved = _repository.Add(new Community
        {
            Name = nameResult.Value!,
            CreatedAt = DateTime.UtcNow
        });

        // log CORRECTO: structured logging, sin interpolation, sin filtrar secretos
        _logger.LogInformation("Comunidad creada {CommunityId} {CommunityName}",
            saved.Id, saved.Name.Value);

        return Result.Ok(saved);
    }
}
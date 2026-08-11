public static class CommunityEndpoints
{
    public static void MapCommunityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/communities");

        group.MapPost("/", (CreateCommunityDto dto, CreateCommunityHandler handler) =>
        {
            var result = handler.Handle(dto.Name);
            return result.IsSuccess
                ? Results.Ok(new { id = result.Value!.Id, name = result.Value!.Name.Value })
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id}", (int id, GetCommunityHandler handler) =>
        {
            var result = handler.Handle(id);
            return result.IsSuccess
                ? Results.Ok(new { id = result.Value!.Id, name = result.Value!.Name.Value })
                : Results.NotFound(result.Error);
        });

        group.MapPut("/{id}/rename", (int id, RenameCommunityDto dto, RenameCommunityHandler handler) =>
        {
            var nameResult = CommunityName.Create(dto.Name);
            if (!nameResult.IsSuccess)
                return Results.BadRequest(nameResult.Error!);

            var result = handler.Handle(id, nameResult.Value!);
            return result.IsSuccess
                ? Results.Ok(new { id = result.Value!.Id, name = result.Value!.Name.Value })
                : Results.NotFound(result.Error);
        });
    }
}

public record CreateCommunityDto(string? Name);
public record RenameCommunityDto(string? Name);
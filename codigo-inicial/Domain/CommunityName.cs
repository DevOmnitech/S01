public sealed record CommunityName
{
    public const int MinLength = 3;    // los numeros magicos, ahora con nombre y en UN lugar
    public const int MaxLength = 21;

    public string Value { get; }

    private CommunityName(string value) => Value = value;   // nadie puede hacer new CommunityName(...)

    public static Result<CommunityName> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result.Fail<CommunityName>("El nombre es requerido.");
        if (raw.Length < MinLength || raw.Length > MaxLength)
            return Result.Fail<CommunityName>($"El nombre debe tener entre {MinLength} y {MaxLength} caracteres.");
        foreach (var ch in raw)
        {
            if (!char.IsLetterOrDigit(ch) && ch != '_')
                return Result.Fail<CommunityName>("Solo se permiten letras, numeros y guion bajo.");
        }
        return Result.Ok(new CommunityName(raw.ToLowerInvariant()));
    }
}
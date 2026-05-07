namespace ClassicHitster.Shared;

public static class CardPayload
{
    public const string SchemePrefix = "classic-hitster://card/";

    public static string Create(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            throw new ArgumentException("Card ID must not be empty.", nameof(cardId));
        }

        return SchemePrefix + Uri.EscapeDataString(cardId.Trim());
    }

    public static string? TryExtractCardId(string? scannedValue)
    {
        if (string.IsNullOrWhiteSpace(scannedValue))
        {
            return null;
        }

        var value = scannedValue.Trim();
        if (value.StartsWith(SchemePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var encodedId = value[SchemePrefix.Length..];
            return string.IsNullOrWhiteSpace(encodedId)
                ? null
                : Uri.UnescapeDataString(encodedId).Trim();
        }

        // Fallback: cards can also contain only the raw ID, e.g. "bach_001".
        return value.Contains(' ')
            ? null
            : value;
    }
}

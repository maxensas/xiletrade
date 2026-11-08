namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>Window of a rate limit rule : at most <paramref name="Max"/> requests in <paramref name="Period"/> seconds.</summary>
/// <param name="Hits">Requests counted by the server in the window when the response was sent.</param>
internal readonly record struct RateLimitRule(int Max, int Period, int Hits);

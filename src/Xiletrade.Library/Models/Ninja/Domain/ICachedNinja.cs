namespace Xiletrade.Library.Models.Ninja.Domain;

public interface ICachedNinja<TContract> : ICachedNinjaItem where TContract : class, new()
{
    void SetJson(TContract json);
    TContract GetJson();
}
using System.Diagnostics.CodeAnalysis;
using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Test.Serialization.Comparer;

public class BaseDataComparer : IEqualityComparer<BaseData>
{
    public bool Equals(BaseData x, BaseData y)
    {
        if (x.Result is null && y.Result is null)
        {
            return true;
        }
        if (x.Result is not null && y.Result is null)
        {
            return false;
        }
        if (x.Result is null && y.Result is not null)
        {
            return false;
        }
        if (x.Result.Length is 0 && y.Result.Length is 0)
        {
            return true;
        }
        if (x.Result.Length != y.Result.Length)
        {
            return false;
        }

        for (int i = 0; i < x.Result.Length; i++)
        {

            var xx = x.Result[i].Data;
            var yy = y.Result[i].Data;

            if (xx is null && yy is null)
            {
                continue;
            }
            if (xx is not null && yy is null)
            {
                return false;
            }
            if (xx is null && yy is not null)
            {
                return false;
            }
            if (xx.Length is 0 && yy.Length is 0)
            {
                continue;
            }
            if (xx.Length != yy.Length)
            {
                return false;
            }

            for (int j = 0; j < xx.Length; j++)
            {
                if (xx[j].Id != yy[j].Id)
                {
                    return false;
                }
                if (xx[j].NameEn != yy[j].NameEn)
                {
                    return false;
                }
                if (xx[j].Name != yy[j].Name)
                {
                    return false;
                }
                if (xx[j].InheritsFrom != yy[j].InheritsFrom)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public int GetHashCode([DisallowNull] BaseData obj)
    {
        throw new NotImplementedException();
    }
}

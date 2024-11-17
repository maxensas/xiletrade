using System.Diagnostics.CodeAnalysis;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Test.Serialization.Comparer;

public class FilterDataComparer : IEqualityComparer<FilterData>
{
    public bool Equals(FilterData x, FilterData y)
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
            if (x.Result[i].Label != y.Result[i].Label)
            {
                return false;
            }
            var xx = x.Result[i].Entries;
            var yy = y.Result[i].Entries;

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
                if (xx[j].ID != yy[j].ID)
                {
                    return false;
                }
                if (xx[j].Part != yy[j].Part)
                {
                    return false;
                }
                if (xx[j].Type != yy[j].Type)
                {
                    return false;
                }
                if (xx[j].Text != yy[j].Text)
                {
                    return false;
                }

                var xxx = xx[j].Option;
                var yyy = yy[j].Option;

                if (xxx is null && yyy is null)
                {
                    continue;
                }
                if (xxx is not null && yyy is null)
                {
                    return false;
                }
                if (xxx is null && yyy is not null)
                {
                    return false;
                }
                var xxxx = xxx.Options;
                var yyyy = yyy.Options;

                if (xxxx is null && yyyy is null)
                {
                    continue;
                }
                if (xxxx is not null && yyyy is null)
                {
                    return false;
                }
                if (xxxx is null && yyyy is not null)
                {
                    return false;
                }
                if (xxxx.Length is 0 && yyyy.Length is 0)
                {
                    continue;
                }
                if (xxxx.Length != yyyy.Length)
                {
                    return false;
                }

                for (int k = 0; k < xxxx.Length; k++)
                {
                    if (xxxx[k].ID.ToString() != yyyy[k].ID.ToString())
                    {
                        return false;
                    }
                    if (xxxx[k].Text != yyyy[k].Text)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public int GetHashCode([DisallowNull] FilterData obj)
    {
        throw new NotImplementedException();
    }
    /*
    public int GetHashCode([DisallowNull] FilterData obj)
    {
       List<string> lblLiist = new();
       List<string> id = new();
       List<string> text = new();
       List<string> part = new();
       List<string> optId = new();
       List<string> optText = new();

       if (obj.Result is null || obj.Result.Length is 0)
       {
           return HashCode.Combine(lblLiist, id, text, part, optId, optText);
       }

       for (int i = 0; i < obj.Result.Length; i++)
       {
           var lbl = obj.Result[i].Label;
           var objSub = obj.Result[i].Entries;

           if (objSub is null)
           {
               return lbl.GetHashCode();
           }
           if (objSub.Length is 0)
           {
               //return HashCode.Combine(lbl, objSub);
               return lbl.GetHashCode();
           }
           lblLiist.Add(lbl);

           for (int j = 0; j < objSub.Length; j++)
           {
               id.Add(objSub[j].ID);
               text.Add(objSub[j].Text);
               part.Add(objSub[j].Part);
               if (objSub[j].Option is not null && objSub[j].Option.Options is not null
                   && objSub[j].Option.Options.Length > 0)
               {
                   foreach (var options in objSub[j].Option.Options)
                   {
                       optId.Add(options.ID.ToString());
                       optText.Add(options.Text);
                   }
               }
           }
       }
       return HashCode.Combine(lblLiist, id, text, part, optId, optText);
    }*/
}

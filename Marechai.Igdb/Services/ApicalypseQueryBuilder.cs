using System.Text;

namespace Marechai.Igdb.Services;

public class ApicalypseQueryBuilder
{
    string _fields = "*";
    string _where;
    string _sort;
    int?   _limit;
    int?   _offset;

    public ApicalypseQueryBuilder Fields(string fields)
    {
        _fields = fields;

        return this;
    }

    public ApicalypseQueryBuilder Where(string where)
    {
        _where = where;

        return this;
    }

    public ApicalypseQueryBuilder Sort(string sort)
    {
        _sort = sort;

        return this;
    }

    public ApicalypseQueryBuilder Limit(int limit)
    {
        _limit = limit;

        return this;
    }

    public ApicalypseQueryBuilder Offset(int offset)
    {
        _offset = offset;

        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();

        sb.Append("fields ").Append(_fields).Append(';');

        if(!string.IsNullOrEmpty(_where))
            sb.Append(" where ").Append(_where).Append(';');

        if(!string.IsNullOrEmpty(_sort))
            sb.Append(" sort ").Append(_sort).Append(';');

        if(_limit.HasValue)
            sb.Append(" limit ").Append(_limit.Value).Append(';');

        if(_offset.HasValue)
            sb.Append(" offset ").Append(_offset.Value).Append(';');

        return sb.ToString();
    }
}

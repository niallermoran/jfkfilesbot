using System.Threading.Tasks;

namespace jfkfiles.bot
{
    public interface ISearchService
    {
        Task<BingSearch> FindArticles(string query);
    }
}

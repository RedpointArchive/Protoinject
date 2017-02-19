using System.Threading.Tasks;

namespace Protoinject
{
    public interface IAsynchronouslyConstructable
    {
        Task ConstructAsync();
    }
}

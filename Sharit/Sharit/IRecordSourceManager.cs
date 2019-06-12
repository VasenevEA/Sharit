using System.Threading.Tasks;

namespace Sharit
{
    public interface IRecordSourceManager
    {
        Task<RecordSource[]> GetSources();
    }
}

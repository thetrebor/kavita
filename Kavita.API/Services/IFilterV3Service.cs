using System.Threading.Tasks;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.API.Services;

public interface IFilterV3Service
{

    Task<FilterResponse> Filter(int userId, FilterV3Dto filter);

    FilterConfigurationDto GetConfiguration();

}

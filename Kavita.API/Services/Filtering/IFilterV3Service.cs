using System.Threading.Tasks;
using Kavita.Models.DTOs.Filtering.v3;

namespace Kavita.API.Services.Filtering;

public interface IFilterService
{

    Task<FilterResponse> Filter(int userId, EntityFilterDto entityFilter);

    FilterConfigurationDto GetConfiguration();

}

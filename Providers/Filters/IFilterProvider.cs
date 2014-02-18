using Orchard.Events;

namespace Orchard.ProjectionFilters.Filters {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }
}
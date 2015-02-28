using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Interfaces
{
    public interface ILogicContainer
    {
        void PreFetch(); //Warn before fetching
        List<ILogical> LogicalList { get; } //Fetch
        void PostFetch(); //Inform the fetch is complete
        void AddLogical(ILogical logical); //Add a logical to the LogicalList
        void RemoveLogical(ILogical logical); //Remove a logical from the LogicalList
    }
}

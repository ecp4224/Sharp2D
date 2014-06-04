using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Logic
{
    public interface ILogicContainer
    {
        List<ILogical> GetLogicalList();
    }
}

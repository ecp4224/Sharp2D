using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D.Core.Logic;

namespace Sharp2D.Core.World
{
    public abstract class World : ILogicContainer
    {
        public List<ILogical> GetLogicalList()
        {
            throw new NotImplementedException();
        }
    }
}

/*
The world API should implement ILogicalContainer.

The world API should have a displayOnScreen method 
that invokes the method in the static Screen class to 
change the Logical Container.

The world API should have methods to add, fetch, and remove
RenderJobs. It should not have API's to add Sprites, that would
be handled by implementations of this World API.
 */
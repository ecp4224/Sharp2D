using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Interfaces
{
    public interface IAttachable
    {
        float X { get; set; }
        float Y { get; set; }
        IList<IAttachable> Children { get; }
        IList<IAttachable> Parents { get; }

        void Attach(IAttachable ToAttach);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Network
{
    public interface Client
    {
        string Username { get; }
        string IP { get; }
        int Port { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientConsole
{
    public interface IClient
    {
        Task<string> SendAsync(string message);
    }
}

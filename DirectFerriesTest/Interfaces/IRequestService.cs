using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFerriesTest.Interfaces
{
    public interface IRequestService
    {
        Task<string> GetAsync(string url, string token);
        Task<string> PutAsync(string url, object data, string token);
    }
}

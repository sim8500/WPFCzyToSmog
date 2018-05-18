using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CzyToSmog.net.Interfaces
{
    public interface IAppNavigation
    {
        void Navigate<T>() where T : class;
        void Navigate<T>(object param) where T : class;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GW.ApiLoader.Utils.Swagger
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HttpOptionsAttribute : Attribute
    {
    }
}

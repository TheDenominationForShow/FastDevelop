using Panda.DynamicWebApi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Panda.DynamicWebApiSample.Dtos
{
    [Autowired]
    public class SayHello
    {
        public string Hello()
        {
            Console.WriteLine("test is ok");
            return "ok";
        }
    }
}

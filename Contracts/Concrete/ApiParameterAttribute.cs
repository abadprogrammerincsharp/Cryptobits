using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Concrete
{
    public class ApiParameterAttribute : Attribute
    {
        public string ParameterName { get; set; }
        public ApiParameterAttribute(string name) => ParameterName = name;
    }
}

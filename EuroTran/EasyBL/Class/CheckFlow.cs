using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyBL
{
    public class CheckFlow
    {
        public string id { set; get; }
        public string Order { set; get; }
        public string SignedWay { set; get; }
        public List<SignedMember> SignedMember { set; get; }
    }

    public class SignedMember
    {
        public string id { set; get; }
        public string name { set; get; }
        public string deptname { set; get; }
        public string jobname { set; get; }
    }
}

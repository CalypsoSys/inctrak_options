using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.Data
{
    public class SaveData<T>
    {
        public Guid Key { get; set; }
        public string UUID { get; set; }
        public T Data { get; set; }
    }
    public class SaveData<T1, T2>
    {
        public Guid Key { get; set; }
        public string UUID { get; set; }
        public T1 Data { get; set; }
        public T2[] Children { get; set; }
    }
}

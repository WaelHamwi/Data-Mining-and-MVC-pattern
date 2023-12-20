using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admtest5.Id3Classes
{
    class Attribut
    {
        public string name { get; set; }
        public List<string> dist_attribut_value { get; set; }
        public int type { get; set; }
        public Tuple<string, int> max_value_and_null_count { get; set; }
        public Attribut(string name, List<string> dist_attribut_value, int type, Tuple<string, int> max_value_and_null_count)
        {
            this.name = name;
            this.dist_attribut_value = dist_attribut_value;
            this.type = type;
            this.max_value_and_null_count = max_value_and_null_count;
        }
    }
}

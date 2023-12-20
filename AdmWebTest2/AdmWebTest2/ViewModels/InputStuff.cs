using AdmWebTest2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdmWebTest2.ViewModels
{
    public class InputStuff
    {
        public C_heart_dataset__ x { get; set; }
        public Tuple<string,double> bayesOutput { get; set; }
        public string id3Output { get; set; }
    }
}
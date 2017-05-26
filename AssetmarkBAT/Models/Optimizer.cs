using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetmarkBAT.Models
{
    public class Optimizer
    {
        public string profitmarginannual { get; set; }
        public string operatingprofitannual { get; set; }
        public string maxvalue { get; set; }
        public string currentmax { get; set; }
        public string currentmin { get; set; }
        public string calculatedmax { get; set; }
        public string calculatedmin { get; set; }
        public string pagr { get; set; }      
        public string vmi { get; set; }
        public string top_pagr_min { get; set; }
        public string top_pagr_max { get; set; }
        public string top_pm_min { get; set; }
        public string top_pm_max { get; set; }
        public string top_vmi_min { get; set; }
        public string top_vmi_max { get; set; }

        public Optimizer()
        {
            top_pagr_min = 8.ToString();
            top_pagr_max = 12.ToString();
            top_pm_min = 20.ToString();
            top_pm_max = 25.ToString();
            top_vmi_min = 700.ToString();
            top_vmi_max = 900.ToString();
        }
    }
}


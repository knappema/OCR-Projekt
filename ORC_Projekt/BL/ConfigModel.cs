using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORC_Projekt.BL
{
    public class ConfigModel
    {
        public ConfigModel()
        {
            ShowDistanceTransformationColored = false;
            ShowSteps = false;
            TemplatePath = @"..\..\Template\";
        }

        public bool ShowDistanceTransformationColored { get; set; }
        public bool ShowSteps { get; set; }
        public string TemplatePath { get; private set; }
    }
}

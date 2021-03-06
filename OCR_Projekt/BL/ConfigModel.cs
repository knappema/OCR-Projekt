﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCR_Projekt.BL
{
    public class ConfigModel
    {
        public ConfigModel()
        {
            ShowDistanceTransformationColored = false;
            ShowSteps = false;
            CreateTemplate = false;

            TemplatePath = @"..\Templates\";
        }

        public bool ShowDistanceTransformationColored { get; set; }
        public bool ShowSteps { get; set; }
        public bool CreateTemplate { get; set; }
        public string TemplatePath { get; private set; }
    }
}

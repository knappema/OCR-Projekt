﻿using System;
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
        }

        public bool ShowDistanceTransformationColored { get; set; }
    }
}

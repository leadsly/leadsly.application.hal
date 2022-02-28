﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Failure
    {
        public Codes? Code { get; set; }
        public string Reason { get; set; }
        public string Detail { get; set; }
    }

    public enum Codes
    {
        WEBDRIVER_ERROR,
        FILE_CLONING_ERROR
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace WTW_Technical_Test
{
    public class TriangleData
    {
        [Name("Product")]
        public string Product { get; set; }

        [Name("Origin Year")]
        public int OriginYear { get; set; }

        [Name("Development Year")]
        public int DevelopmentYear { get; set; }

        [Name("Incremental Value")]
        public decimal IncrementalValue { get; set; }

    }
}

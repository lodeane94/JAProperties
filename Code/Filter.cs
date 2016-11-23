using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Controllers
{
    public enum Op
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        Like
    }

    public class Filter
    {
        public String PropertyName { get; set; }
        public Op Operation { get; set; }
        public Object Value { get; set; }
    }
}
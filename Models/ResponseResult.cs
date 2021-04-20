using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyDocker.Models
{
    public class ResponseResult
    {
        public int code { set; get; }
        public string message { set; get; }
        public Exception exception { set; get; }
    }
}

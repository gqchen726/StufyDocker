using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyDocker.Services;
using StudyDocker.Models;

namespace StudyDocker.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly StudyRedisContext _context;
        private IAccessHistoryLogService _service { get; set; }

        public TestController(StudyRedisContext context, IAccessHistoryLogService service)
        {
            _context = context;
            _service = service;
        }
        [Microsoft.AspNetCore.Mvc.HttpGet("Test1")]
        public string Test1(string name)
        {
           
            return "Test1"+name;
        }
    }
}

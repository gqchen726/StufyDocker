using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyDocker.Models;
using StudyDocker.Services;
using Microsoft.AspNetCore.Cors;
using OfficeOpenXml;
using System.IO;
using System.Net.Http;

namespace StudyDocker.Controllers
{
    /*[Route("api/[controller]/[action]", Name = "[controller]_[action]")]*/
    [Route("api/[controller]")]
    [ApiController]
    public class AccessHistoryLogsController : ControllerBase
    {
        private readonly StudyRedisContext _context;
        
        private IAccessHistoryLogService _service { get; set; }

        public AccessHistoryLogsController(StudyRedisContext context,IAccessHistoryLogService service)
        {
            _context = context;
            _service = service;
        }

        // GET: api/AccessHistoryLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccessHistoryLog>>> GetAccessHistoryLogs()
        {
            return await _context.AccessHistoryLogs.ToListAsync();
        }


        [HttpGet("getPV")]
        public long GetPageView()
        {
            _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
            return _service.CountOfPageView();
        }


        [HttpGet("getUV")]
        public long GetUniqueVisiter()
        {
            _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
            return _service.CountOfUniqueVisitor();
        }
        [HttpGet("getPVOfCurrentPage/{path}")]
        public long GetPageViewOfCurrentPage(string Path)
        {
            /*string path =  HttpContext.Request.Path;*/
            _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
            string path = "/api/AccessHistoryLogs/" + Path;
            return _service.CountOfPageViewForPagePath(path);
        }


        [HttpGet("getUVOfCurrentPage/{path}")]
        public long GetUniqueVisiterOfCurrentPage(string Path)
        {
            /*string path = HttpContext.Request.Path;*/
            _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
            string path = "/api/AccessHistoryLogs/" + Path;
            return _service.CountOfUniqueVisitorForPagePath(path);
        }

        [HttpGet("getAll")]
        public Task<List<AccessHistoryLog>> GetAllAccessHistoryLog()
        {
            _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
            return _service.FindAllAccessHistoryLog();
        }
        [EnableCors("myCors")]
        [HttpGet("access")]
        public string AddAccessHistoryLog()
        {
            return _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
        }
        [HttpGet("access/{path}")]
        public string AddAccessHistoryLogOfSpecialPath(string path)
        {
            string Path = "/api/AccessHistoryLogs/" + path;
            HttpContext.Request.Path = Path;
            return _service.AddAccessHistoryLogOfRedisWithEntity(HttpContext);
        }
        /// <summary>
        ///使用ClosedXML NuGet包
        /// </summary>
        /// <returns></returns>
        /*[HttpPost("export")]
        public IActionResult exportLogs()
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "accessHistoryLogs.xlsx";
            IList<AccessHistoryLog> accessHistoryLogs;
            try
            {
                accessHistoryLogs = _service.GetAccessHistoryLogs();
                IXLWorkbook workbook = _service.exportLogs(new XLWorkbook(), accessHistoryLogs, "AccessHistoryLog");
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }*/

        /// <summary>
        ///使用EPPlus NuGet包
        /// </summary>
        /// <returns></returns>
        [HttpPost("exportOfEPP")]
        public IActionResult exportOfEPP()
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "accessHistoryLogs.xlsx";
            IList<AccessHistoryLog> accessHistoryLogs;
            try
            {
                accessHistoryLogs = _service.GetAccessHistoryLogs();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                FileInfo file = new FileInfo(@"数据.xlsx");

                ExcelPackage package = new ExcelPackage(file);
                ExcelWorkbook workbook = package.Workbook;
                _service.exportLogs(workbook, accessHistoryLogs, "AccessHistoryLog");

                using (var stream = new System.IO.MemoryStream())
                {
                    
                    package.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpPost("exportOfEPP/{path}")]
        public IActionResult exportOfEPPOfPagination(string path, int index, int size)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "accessHistoryLogs.xlsx";
            IList<AccessHistoryLog> accessHistoryLogs;
            try
            {
                accessHistoryLogs = _service.GetAccessHistoryLogsOfPagination(path, index, size);
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                FileInfo file = new FileInfo(@"数据.xlsx");

                ExcelPackage package = new ExcelPackage(file);
                ExcelWorkbook workbook = package.Workbook;
                _service.exportLogs(workbook, accessHistoryLogs, "AccessHistoryLog");

                using (var stream = new System.IO.MemoryStream())
                {

                    package.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("access1")]
        public AccessHistoryLog AddAccessHistoryLog1()
        {
            _service.AddAccessHistoryLogOfRedis(HttpContext);
            return _service.AddAccessHistoryLog1(HttpContext);
        }
        [HttpPost("send")]
        public async Task<ResponseResult> SendTextAsync(Test test)
        {
            Console.WriteLine(test.name);
            ResponseResult result = new ResponseResult();
            result.message = "上传成功";
            result.code = 1;
            return result;
        }
        [HttpPost("sendAll")]
        public async Task<ResponseResult> SendAllTextAsync(IList<Test> tests)
        {
            Console.WriteLine(tests[0].name);
            ResponseResult result = new ResponseResult();
            result.message = "批量上传成功";
            result.code = 1;
            return result;
        }




        // GET: api/AccessHistoryLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AccessHistoryLog>> GetAccessHistoryLog(int id)
        {
            var accessHistoryLog = await _context.AccessHistoryLogs.FindAsync(id);

            if (accessHistoryLog == null)
            {
                return NotFound();
            }

            return accessHistoryLog;
        }

        // PUT: api/AccessHistoryLogs/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccessHistoryLog(int id, AccessHistoryLog accessHistoryLog)
        {
            if (id != accessHistoryLog.Id)
            {
                return BadRequest();
            }

            _context.Entry(accessHistoryLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessHistoryLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AccessHistoryLogs
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AccessHistoryLog>> PostAccessHistoryLog(AccessHistoryLog accessHistoryLog)
        {
            _context.AccessHistoryLogs.Add(accessHistoryLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccessHistoryLog", new { id = accessHistoryLog.Id }, accessHistoryLog);
        }

        // DELETE: api/AccessHistoryLogs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AccessHistoryLog>> DeleteAccessHistoryLog(int id)
        {
            var accessHistoryLog = await _context.AccessHistoryLogs.FindAsync(id);
            if (accessHistoryLog == null)
            {
                return NotFound();
            }

            _context.AccessHistoryLogs.Remove(accessHistoryLog);
            await _context.SaveChangesAsync();

            return accessHistoryLog;
        }

        private bool AccessHistoryLogExists(int id)
        {
            return _context.AccessHistoryLogs.Any(e => e.Id == id);
        }

    }
}

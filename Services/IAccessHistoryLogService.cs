using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyDocker.Models;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using OfficeOpenXml;

namespace StudyDocker.Services
{
    public interface IAccessHistoryLogService
    {
        string AddAccessHistoryLog(AccessHistoryLog newAccessHistoryLog);
        string AddAccessHistoryLogOfEntity(HttpContext httpContext, string sessionId);
        Task<List<AccessHistoryLog>> FindAllAccessHistoryLog();
        List<AccessHistoryLog> GetAccessHistoryLogs();
        IList<AccessHistoryLog> GetAccessHistoryLogsOfPagination(string path, int pageIndex, int pageSize);
        ///public IXLWorkbook exportLogs(IXLWorkbook workbook, IList<AccessHistoryLog> datas, string dataName);
        public ExcelWorkbook exportLogs(ExcelWorkbook workbook, IList<AccessHistoryLog> datas, string dataName);
        long CountOfPageView();
        long CountOfPageViewForPagePath(string Path);
        long CountOfUniqueVisitor();
        long CountOfUniqueVisitorForPagePath(string Path);
        AccessHistoryLog AddAccessHistoryLog1(HttpContext httpContext);
        ///string AddAccessHistoryLogOfScope(HttpContext httpContext,AccessHistoryLog accessHistoryLog);

        string AddAccessHistoryLogOfRedis(HttpContext httpContext);
        string AddAccessHistoryLogOfRedisWithEntity(HttpContext httpContext);
    }
}

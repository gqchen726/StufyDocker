using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyDocker.Models;
using StackExchange.Redis;
using ClosedXML.Excel;
using System.Collections;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace StudyDocker.Services
{
    public class AccessHistoryLogService : IAccessHistoryLogService
    {
        public StudyRedisContext _context;
        public AccessHistoryLog _accessHistoryLog;
        public IDatabase _redis;
        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="context"></param>
        public AccessHistoryLogService(StudyRedisContext context,RedisHelper redis)
        {
            _context = context;
            _redis = redis.GetDatabase();
        }
        public string AddAccessHistoryLog(AccessHistoryLog newAccessHistoryLog)
        {
            string message = "添加成功";
            bool result = true;
            try
            {
                _context.AccessHistoryLogs.Add(newAccessHistoryLog);
                _context.SaveChanges();
            } catch
            {
                message = "添加失败";
                result = false;
                return $"{message}:{result}";
            }

            return $"{message}:{result}";
        }

        /// <summary>
        /// entity class不使用依赖注入
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>返回string类型的字符串，格式为"message:result"</returns>
        public string AddAccessHistoryLogOfEntity(HttpContext httpContext,string sessionId)
        {
            string Path = httpContext.Request.Path;
            
            _accessHistoryLog = new AccessHistoryLog();
            _accessHistoryLog.IpAddress = httpContext.Connection.RemoteIpAddress.ToString();
            _accessHistoryLog.DateTime = DateTime.Now;
            _accessHistoryLog.AccessPath = Path;
            _accessHistoryLog.SessionId = sessionId;

            return AddAccessHistoryLog(_accessHistoryLog);
        }

        public string AddAccessHistoryLogOfRedis(HttpContext httpContext)
        {
            string Path = httpContext.Request.Path;
            string sessionId;
            if (!httpContext.Request.Cookies.ContainsKey("session-id"))
            {
                ///ISession session = httpContext.Session;
                /// 使用session,缺陷：有可能生成的session为空
                ///httpContext.Response.Cookies.Append("session-id", session.Id);
                /// sessionId = session.Id;
                /// 使用GUID(UUID)
                sessionId = Guid.NewGuid().ToString();
                httpContext.Response.Cookies.Append("session-id", sessionId);
                /// 使用时间戳
                /// sessionId = DateTime.Now.ToString("yyyyMMddhhmmssffff");
            }
            else
            {
                sessionId = httpContext.Request.Cookies["session-id"];
            }

            this.addTotal(sessionId);
            this.addSpecial(Path,sessionId);
            return sessionId;
        }
        public string AddAccessHistoryLogOfRedisWithEntity(HttpContext httpContext)
        {
            string sessionId = this.AddAccessHistoryLogOfRedis(httpContext);
            return this.AddAccessHistoryLogOfEntity(httpContext, sessionId);
        }

        /// <summary>
        /// entity class不使用依赖注入
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>成功时返回新增的记录</returns>
        public AccessHistoryLog AddAccessHistoryLog1(HttpContext httpContext)
        {

            ///读取Cookie，根据Cookie来判断是否计入有效点击（追踪其记录访问）

            ///判断Cooike是否存在于redis中，存在不计入缓存
            /// 如果不存在则放入Cookie中
            /// 在数据库持久化
            /// redis:
            ///     sessions:
            ///         session1
            ///         session2
            ///     pvs:
            ///         sessionId1 ：accessPath1
            ///         sessionId2 ：accessPath2
            ///     uvs:
            ///         path1: 1
            ///         path2: 1
            /// 首先记录pv
            ///     然后判断请求中是否携带sesionId
            ///         false：生成并返回、记录sessionId和uv
            ///         true：判断该sessionId是否存在于sessions中
            ///             true：
            ///             false：记录sessionId和uv
            /// sessionId ：accessPath
            string Path = httpContext.Request.Path;


            /*string sessionId;
            if (!httpContext.Request.Cookies.ContainsKey("session-id"))
            {
                ISession session = httpContext.Session;
                httpContext.Response.Cookies.Append("session-id", session.Id);
                sessionId = session.Id;
            } else
            {
                sessionId = httpContext.Request.Cookies["session-id"];
            }

            this.addTotal(sessionId);
            this.addSpecial(sessionId,Path);*/


            /*if(!httpContext.Request.Cookies.ContainsKey("session-id"))
            {
                ISession session = httpContext.Session;
                httpContext.Response.Cookies.Append("session-id", session.Id);
                _redis.ListRightPush("sessions", session.Id);

                RedisValue path = _redis.StringGet(Path);
                if (path.IsNullOrEmpty)
                {
                    _redis.StringSet(Path, 0);
                }
                _redis.StringIncrement(Path, 1);
            } else
            {
                Boolean isExists = false;
                RedisValue[] sessions = _redis.ListRange("sessions");
                if (sessions.Length != 0)
                {
                    for (int i = 0; i < sessions.Length; i++)
                    {
                        isExists = httpContext.Request.Cookies.TryGetValue("session-id",out string v);
                     }
                }

                if(!isExists)
                {
                    RedisValue path = _redis.StringGet(Path);
                    _redis.ListRightPush("sessions", httpContext.Request.Cookies["session-id"]);
                    if (path.IsNullOrEmpty)
                    {
                        _redis.StringSet(Path, 0);
                    }
                    _redis.StringIncrement(Path, 1);
                }
            }*/



            _accessHistoryLog = new AccessHistoryLog();
            _accessHistoryLog.IpAddress = httpContext.Connection.RemoteIpAddress.ToString();
            _accessHistoryLog.DateTime = DateTime.Now;
            _accessHistoryLog.AccessPath = Path;

            _context.AccessHistoryLogs.Add(_accessHistoryLog);

            try
            {
                _context.SaveChanges();
            }
            catch
            {
                return null;
            }

            return _accessHistoryLog;
        }


        /// <summary>
        /// entity class使用依赖注入，实体类的实例不交给容器来管理，已弃用
        /// 使用services.AddScope注册服务，优势是每次请求都会获取一个实例
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="_accessHistoryLog"></param>
        /// <returns></returns>
        /*public string AddAccessHistoryLogOfScope(HttpContext httpContext, AccessHistoryLog _accessHistoryLog)
        {
            _accessHistoryLog.IpAddress = httpContext.Connection.RemoteIpAddress.ToString();
            _accessHistoryLog.DateTime = DateTime.Now;


            return AddAccessHistoryLog(_accessHistoryLog);
        }*/

        public long CountOfPageView()
        {
            return this.getTotalOfPv();
            ///return _context.AccessHistoryLogs.Select(p => p.IpAddress).Count();
        }

        public long CountOfPageViewForPagePath(string Path)
        {


            return this.getSpecialOfPv(Path);
        }

        public long CountOfUniqueVisitor()
        {
            return this.getTotalOfUv();
            ///return _context.AccessHistoryLogs.Select(p => p.IpAddress).Distinct().Count();
        }

        public long CountOfUniqueVisitorForPagePath(string Path)
        {
            return this.getSpecialOfUv(Path);
        }

        public Task<List<AccessHistoryLog>> FindAllAccessHistoryLog()
        {
            ///LINQTest();
            return _context.AccessHistoryLogs.ToListAsync();
        }


        private void addTotalOfPv(string sessionId)
        {
            _redis.ListRightPush("totalOfPv",sessionId);
        }
        private long getTotalOfPv()
        {
            return _redis.ListLength("totalOfPv");
        }
        private void addTotalOfUv(string sessionId)
        {
            _redis.SetAdd("totalOfUv",sessionId);
        }
        private long getTotalOfUv()
        {
            return _redis.SetLength("totalOfUv");
        }
        private void addTotal(string sessionId)
        {
            this.addTotalOfPv(sessionId);
            this.addTotalOfUv(sessionId);
        }
        private void addSpecialOfPv(string Path,string sessionId)
        {
            string specialOfPv = Path + "/Pv";
            _redis.ListRightPush(specialOfPv, sessionId);
        }
        private long getSpecialOfPv(string Path)
        {
            string specialOfPv = Path + "/Pv";
            return _redis.ListLength(specialOfPv);
        }
        private void addSpecialOfUv(string Path,string sessionId)
        {
            string specialOfUv = Path + "/Uv";
            _redis.SetAdd(specialOfUv, sessionId);
        }
        private long getSpecialOfUv(string Path)
        {
            string specialOfUv = Path + "/Uv";
            return _redis.SetLength(specialOfUv);
        }
        private void addSpecial(string Path, string sessionId)
        {
            this.addSpecialOfPv(Path,sessionId);
            this.addSpecialOfUv(Path,sessionId);
        }

        public List<AccessHistoryLog> GetAccessHistoryLogs()
        {
            return _context.AccessHistoryLogs.ToList();
        }

        /// <summary>
        ///使用ClosedXML NuGet包
        /// </summary>
        /// <returns></returns>
        /*public IXLWorkbook exportLogs(IXLWorkbook workbook,IList<AccessHistoryLog> datas,string dataName)
        {
            *//*Assembly assembly = Assembly.Load("SomeLibrary");
            Type type = assembly.GetType("AccessHistoryLog");*//*
            Type type = Type.GetType("StudyDocker.Models.AccessHistoryLog");
            PropertyInfo[] properties =  type.GetProperties();
            try
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add(type.Name);
                int i = 1;
                int header = 1;
                foreach (var property in properties)
                {
                    string value = property.Name.ToString(); 
                    worksheet.Cell(i, header++).Value = value;
                    worksheet.ColumnWidth = value.Length;
                }
                foreach (var data in datas)
                {
                    for (int j = 0; j < properties.Length; )
                    {
                        PropertyInfo property = properties[j];
                        string value = property.GetValue(data).ToString();
                        worksheet.Cell(i + 1, ++j).SetValue(value);
                        ///worksheet.ColumnWidth = value.Length;
                    }
                    i++;
                };
                return workbook;
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
        public ExcelWorkbook exportLogs(ExcelWorkbook workbook, IList<AccessHistoryLog> datas, string dataName)
        {
            /*Assembly assembly = Assembly.Load("SomeLibrary");
            Type type = assembly.GetType("AccessHistoryLog");*/
            Type type = Type.GetType("StudyDocker.Models.AccessHistoryLog");
            PropertyInfo[] properties = type.GetProperties();
            try
            {
                ///IXLWorksheet worksheet = workbook.Worksheets.Add(type.Name);
                ExcelWorksheet worksheet = workbook.Worksheets.Add("sheet1");
                int i = 1;
                int header = 1;
                foreach (var property in properties)
                {
                    string value = property.Name.ToString();
                    ExcelRange excelRange = worksheet.Cells[i, header++];
                    excelRange.Value = value;
                    excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;//垂直居中
                    excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;//水平靠左
                }
                foreach (var data in datas)
                {
                    for (int j = 0; j < properties.Length;)
                    {
                        PropertyInfo property = properties[j];
                        string value = property.GetValue(data).ToString();
                        ExcelRange excelRange = worksheet.Cells[i + 1, ++j];
                        excelRange.Value = value;
                        excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;//垂直居中
                        excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;//水平靠左

                    }
                    i++;
                };
                worksheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小
                return workbook;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// LINQ的学习
        /// </summary>
        public void LINQTest()
        {
            IEnumerable<AccessHistoryLog> datas = System.Linq.Enumerable.Where(
                _context.AccessHistoryLogs, n => n.Id <= 100);
            IEnumerable<AccessHistoryLog> result =
                from n in datas
                where n.SessionId.Contains("a5")
                orderby n.Id
                select n;
            foreach (var data in result)
            {
                Console.WriteLine(data.ToString());
            }
        }

        public IList<AccessHistoryLog> GetAccessHistoryLogsOfPagination(string path, int pageIndex, int pageSize)
        {
            string newPath = "/api/AccessHistoryLogs/" + path;
            
            var result =
               (
                     from n in _context.AccessHistoryLogs
                     where n.AccessPath == newPath
                     orderby n.Id
                     select n
               );
            var datas = result.Where(n => n.AccessPath == newPath).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            /*foreach (var data in accessHistoryLogs)
            {
                Console.WriteLine(data.ToString());
            }*/
            return datas.ToList<AccessHistoryLog>();
        }
    }
}

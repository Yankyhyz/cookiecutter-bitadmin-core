using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitAdminCore.Helpers;
using BitAdminCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitAdminCore.Controllers
{
    public class DemoOneController : Controller
    {

        DataContext dbContext = new DataContext();
        /// <summary>
        /// 获取页面操作数据
        /// </summary>
        /// <returns></returns>
        public JsonResult QueryDemoOneData(int offset, int limit)
        {
            try
            {
                var list = dbContext.Set<DemoOne>().OrderBy(a => a.CreateTime).Skip(offset).Take(limit).ToList();
                return Json(new { Code = 0, Total = dbContext.Set<DemoOne>().Count(), Data = list });
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }

        /// <summary>
        /// 保存页面操作(新增、修改)
        /// </summary>
        /// <returns></returns>
        public JsonResult SaveDemoOneData(Guid? id)
        {
            try
            {
                //标识唯一性验证
                var smodel = dbContext.Set<DemoOne>().FirstOrDefault(so => so.Id != id);
                if (smodel != null)
                    return Json(new { Code = 1, Msg = "该标识已存在！" });

                DemoOne model = dbContext.Set<DemoOne>().FirstOrDefault(so => so.Id == id);
                if (model == null)
                {
                    model = new DemoOne();
                    this.ToModel(model);
                    model.Id = Guid.NewGuid();
                    model.CreateUserId = SSOClient.UserId;
                    model.CreateTime = DateTime.Now;
                    dbContext.Set<DemoOne>().Add(model);
                }
                else
                {
                    this.ToModel(model);
                }

                dbContext.SaveChanges();
                return Json(new { Code = 0, Msg = "保存成功！" });

            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }

        /// <summary>
        /// 加载页面操作数据
        /// </summary>
        /// <returns></returns>
        public JsonResult LoadDemoOneData(Guid id)
        {
            try
            {
                var model = dbContext.Set<DemoOne>().FirstOrDefault(so => so.Id == id);
                return Json(new { Code = 0, Data = model });
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }

        /// <summary>
        /// 删除页面操作
        /// </summary>
        /// <returns></returns>
        public JsonResult DeleteDemoOneData(string IDs)
        {
            try
            {
                var result = SqlHelper.ExecuteSql(QuerySuite.DeleteSql(IDs, "DemoOne", "id"));
                return Json(new { Code = 0, Msg = "删除成功！" });
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }
    }
}
/***********************
 * BitAdmin2.0框架文件
 * 登录权限公共功能
 ***********************/
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BitAdminCore.Helpers;
using BitAdminCore.Models;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace BitAdminCore.Controllers
{
    public class AuthController : Controller
    {
        DataContext dbContext = new DataContext();

        public JsonResult BindUser(string account, string password, string openId)
        {
            try
            {
                if (!SSOClient.Validate(account, password, out Guid userId))
                    return Json(new { Code = 1, Msg = "帐号或密码不正确，请重新输入！" });
                //公众号绑定
                SysUserOpenId userOpenId = dbContext.Set<SysUserOpenId>().Where(x => x.OpenId == openId).FirstOrDefault();
                if (userOpenId != null)
                {
                    userOpenId.UserId = userId;
                    userOpenId.BindTime = DateTime.Now;
                }
                else
                {
                    userOpenId = new SysUserOpenId();
                    userOpenId.OpenId = openId;
                    userOpenId.UserId = userId;
                    userOpenId.CreateTime = DateTime.Now;
                    userOpenId.BindTime = DateTime.Now;
                    dbContext.SysUserOpenId.Add(userOpenId);
                }
                dbContext.SaveChanges();

                SSOClient.SignIn(userId);
                return Json(new { Code = 0 });
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }

        #region QQ互联登录
        static string qq_access_token;
        static string qq_refresh_token;
        static DateTime qq_expires_time;
        public ActionResult QQSignIn(string code)
        {
            try
            {
                /* 作者说明：续期功能未测试，有谁帮忙测一下。
                 */
                if (string.IsNullOrEmpty(code))
                    return Json(new { code = 1, Msg = "参数错误" });

                //QQ互联配置信息
                string appid = "101107448";
                string appkey = "ae7af6e66a8655f5dce06dce7fe20859";
                string status = "";
                string reurl = "https%3A%2F%2Fwww.bitadmincore.com%2Fauth%2Fqqsignin";
                string url, json;

                WebClient wcl = new WebClient();
                /*access_token有次数限制，不能每次都取一个新的，在有效期内可以使用，过了有效期则续期就可以了。*/
                if (string.IsNullOrEmpty(qq_access_token))
                {
                    url = string.Format("https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&client_id={0}&client_secret={1}&code={2}&state={3}&redirect_uri={4}", appid, appkey, code, status, reurl);
                    json = wcl.DownloadString(url);
                    //string token = "access_token=06B07A1CA3144B67BB8ECE03577E4DC5&expires_in=7776000&refresh_token=8DB41D424D74B1F79322946F0E3A17B0";
                    var param = QueryHelpers.ParseQuery(json);
                    qq_access_token = param["access_token"];
                    qq_refresh_token = param["refresh_token"];
                    qq_expires_time = DateTime.Now.AddSeconds(Convert.ToInt32(param["expires_in"].ToString()));
                }
                else if (qq_expires_time < DateTime.Now.AddMinutes(30))//提前30分钟续期
                {
                    url = string.Format("https://graph.qq.com/oauth2.0/token?grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}", appid, appkey, qq_refresh_token);
                    json = wcl.DownloadString(url);
                    //string token = "access_token=06B07A1CA3144B67BB8ECE03577E4DC5&expires_in=7776000&refresh_token=8DB41D424D74B1F79322946F0E3A17B0";
                    var param = QueryHelpers.ParseQuery(json);
                    qq_access_token = param["access_token"];
                    qq_refresh_token = param["refresh_token"];
                    qq_expires_time = DateTime.Now.AddSeconds(Convert.ToInt32(param["expires_in"].ToString()));
                }

                url = "https://graph.qq.com/oauth2.0/me?access_token=" + qq_access_token;
                json = wcl.DownloadString(url);
                //json = "callback( {\"client_id\":\"101107448\",\"openid\":\"1B09CF38A9D917645272095DFF8B6074\"} );";
                var mc = new Regex("(?<=\\u0028).*?(?= \\u0029)", RegexOptions.None).Matches(json)[0];//提取出json对象
                JObject me = JObject.Parse(mc.Value);
                string client_id = (string)me["client_id"];
                string openId = (string)me["openid"];

                SysUserOpenId userOpenId = dbContext.Set<SysUserOpenId>().Where(x => x.OpenId == openId).FirstOrDefault();
                if (userOpenId != null && userOpenId.UserId != Guid.Empty)
                {
                    SSOClient.SignIn(userOpenId.UserId.Value);
                    return Redirect("/pages/home/index.html");
                }
                return Redirect("/pages/account/bind.html?sign=pc&openid=" + openId);

                //自动创建本地用户，适用面向公众网站，项目根据需要调整逻辑。
                //url = string.Format("https://graph.qq.com/user/get_user_info?access_token={0}&oauth_consumer_key={1}&openid={2}", access_token, appid, openId);
                //wcl.Encoding = Encoding.UTF8;
                //json = wcl.DownloadString(url);
                //JObject qqUser = JObject.Parse(json.Replace("\\", ""));

                //var user = new SysUser();
                //user.UserId = Guid.NewGuid();
                //user.UserName = (string)qqUser["nickname"];//更多可用属性请查看官方接入文档
                //user.UserCode = Guid.NewGuid().ToString("N").Substring(20);
                //user.DepartmentId = new Guid("2379788E-45F0-417B-A103-0B6440A9D55D");
                //dbContext.SysUser.Add(user);

                //userOpenId = new SysUserOpenId();
                //userOpenId.OpenId = openId;
                //userOpenId.UserId = user.UserId;
                //userOpenId.CreateTime = DateTime.Now;
                //userOpenId.BindTime = DateTime.Now;
                //dbContext.SysUserOpenId.Add(userOpenId);

                //dbContext.SaveChanges();

                //SSOClient.SignIn(userOpenId.UserId.Value);
                //return Redirect("/pages/home/index.html");

            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }
        #endregion

        #region 微信互联登录
        static string weixin_access_token;
        static string weixin_refresh_token;
        static DateTime weixin_expires_time;
        public ActionResult WeixinSignIn(string code)
        {
            try
            {
                /* 作者说明：因没有账号，本功能未测试。
                 * 
                 * 作者疑问？？
                 * 微信互联登录access_token逻辑感觉有问题：请求access_token时返回openid，那么access_token是跟appid绑定？还是跟openid绑定？
                 * 如果是跟openid绑定，那么需要维护一个access_token与openid关系及时效表，用户关闭应用重新启动登录时，只能重新获取code再获取openid，意义何在？
                 */

                if (string.IsNullOrEmpty(code))
                    return Json(new { code = 1, Msg = "参数错误" });

                //微信互联配置信息
                string appid = "";
                string appkey = "";
                string url, json, openid = string.Empty, unionid=string.Empty;//你要用openid还是用unionid请详读官网说明。

                WebClient wcl = new WebClient();

                /*access_token有次数限制，不能每次都取一个新的，在有效期内可以使用，过了有效期则续期就可以了。*/
                if (string.IsNullOrEmpty(weixin_access_token))
                {
                    url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", appid, appkey, code);
                    json = wcl.DownloadString(url);
                    JObject weixinToken = JObject.Parse(json.Replace("\\", ""));
                    openid = (string)weixinToken["openid"];//更多可用属性请查看官方接入文档(access_token,expires_in,refresh_token,openid,scope,unionid)
                    unionid = (string)weixinToken["unionid"];

                    weixin_access_token = (string)weixinToken["access_token"];
                    weixin_refresh_token = (string)weixinToken["refresh_token"];
                    weixin_expires_time = DateTime.Now.AddSeconds(Convert.ToInt32(weixinToken["expires_in"].ToString()));
                }
                else if (weixin_expires_time < DateTime.Now.AddMinutes(30))//提前30分钟续期
                {
                    url = string.Format("https://api.weixin.qq.com/sns/oauth2/refresh_token?grant_type=refresh_token&appid={0}&refresh_token={1}", appid, weixin_refresh_token);
                    json = wcl.DownloadString(url);
                    //string token = "access_token=06B07A1CA3144B67BB8ECE03577E4DC5&expires_in=7776000&refresh_token=8DB41D424D74B1F79322946F0E3A17B0";
                    var param = QueryHelpers.ParseQuery(json);
                    weixin_access_token = param["access_token"];
                    weixin_refresh_token = param["refresh_token"];
                    weixin_expires_time = DateTime.Now.AddSeconds(Convert.ToInt32(param["expires_in"].ToString()));
                }

                SysUserOpenId userOpenId = dbContext.Set<SysUserOpenId>().Where(x => x.OpenId == openid).FirstOrDefault();
                if (userOpenId != null && userOpenId.UserId != Guid.Empty)
                {
                    SSOClient.SignIn(userOpenId.UserId.Value);
                    return Redirect("/pages/home/index.html");
                }
                return Redirect("/pages/account/bind.html?sign=pc&openid=" + openid);

                //自动创建本地用户，适用面向公众网站，项目根据需要调整逻辑。
                //url = string.Format("https://api.weixin.qq.com//sns/userinfo?access_token={0}&openid={1}", weixin_access_token, appid, openid);
                //wcl.Encoding = Encoding.UTF8;
                //json = wcl.DownloadString(url);
                //JObject weixinUser = JObject.Parse(json.Replace("\\", ""));

                //var user = new SysUser();
                //user.UserId = Guid.NewGuid();
                //user.UserName = (string)weixinUser["nickname"];//更多可用属性请查看官方接入文档(openid,nickname,sex,province,city,country,headimgurl,privilege[],unionid)
                //user.UserCode = Guid.NewGuid().ToString("N").Substring(20);
                //user.DepartmentId = new Guid("2379788E-45F0-417B-A103-0B6440A9D55D");
                //dbContext.SysUser.Add(user);

                //userOpenId = new SysUserOpenId();
                //userOpenId.OpenId = openid;
                //userOpenId.UserId = user.UserId;
                //userOpenId.CreateTime = DateTime.Now;
                //userOpenId.BindTime = DateTime.Now;
                //dbContext.SysUserOpenId.Add(userOpenId);

                //dbContext.SaveChanges();

                //SSOClient.SignIn(userOpenId.UserId.Value);
                //return Redirect("/pages/home/index.html");

            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(ex);
                return Json(new { Code = 1, Msg = "服务器异常，请联系管理员！" });
            }
        }
        #endregion        
    }
}
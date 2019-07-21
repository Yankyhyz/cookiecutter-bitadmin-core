/***********************
 * BitAdmin2.0框架文件
 ***********************/

namespace BitAdminCore.Helpers
{
    public class SMSService
    {
        public static bool Send(string mobile,string msg)
        {
            LogHelper.SaveLog("sms", string.Format("发送信息：{0}:{1}", mobile, msg));
            return true;
        }
    }
}

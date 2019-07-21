/***********************
 * BitAdmin2.0框架文件
 ***********************/
using BitAdminCore.UEditor;
using Microsoft.AspNetCore.Mvc;

namespace BitAdminCore.Controllers
{
    [Route("[controller]")]
    public class UEditorController : Controller
    {
        private UEditorService ue;
        public UEditorController(UEditorService ue)
        {
            this.ue = ue;
        }

        public void Do()
        {
            ue.DoAction(HttpContext);
        }
    }
}

using System.Collections.Generic;
using System.Web.Mvc;

namespace Estream.Cart42.Web.Helpers
{
    public class Alert
    {
        public Alert(string alertClass, string message)
        {
            AlertClass = alertClass;
            Message = message;
        }

        public string AlertClass { get; set; }
        public string Message { get; set; }
    }

    public static class AlertExtensions
    {
        private const string Alerts = "_Alerts";

        public static List<Alert> GetAlerts(this TempDataDictionary tempData)
        {
            if (!tempData.ContainsKey(Alerts))
            {
                tempData[Alerts] = new List<Alert>();
            }

            return (List<Alert>) tempData[Alerts];
        }

        public static ActionResult WithSuccess(this ActionResult result, string message)
        {
            return new AlertDecoratorResult(result, "alert-success", message);
        }

        public static ActionResult WithInfo(this ActionResult result, string message)
        {
            return new AlertDecoratorResult(result, "alert-info", message);
        }

        public static ActionResult WithWarning(this ActionResult result, string message)
        {
            return new AlertDecoratorResult(result, "alert-warning", message);
        }

        public static ActionResult WithError(this ActionResult result, string message)
        {
            return new AlertDecoratorResult(result, "alert-danger", message);
        }
    }

    public class AlertDecoratorResult : ActionResult
    {
        public AlertDecoratorResult(ActionResult innerResult, string alertClass, string message)
        {
            InnerResult = innerResult;
            AlertClass = alertClass;
            Message = message;
        }

        public ActionResult InnerResult { get; set; }
        public string AlertClass { get; set; }
        public string Message { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            List<Alert> alerts = context.Controller.TempData.GetAlerts();
            alerts.Add(new Alert(AlertClass, Message));
            InnerResult.ExecuteResult(context);
        }
    }
}
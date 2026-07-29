using CommonLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Qualification.Controllers
{
    public class UserDetailController(ICommonLogger logger) : Controller
    {
        // GET: UserDetailController
        public ActionResult Index()
        {
            logger.LogInformation("UserDetail Index page requested");
            return View();
        }

        // GET: UserDetailController/Details/5
        public ActionResult Details(int id)
        {
            logger.LogInformation($"UserDetail Details requested for ID: {id}");
            return View();
        }

        // GET: UserDetailController/Create
        public ActionResult Create()
        {
            logger.LogInformation("UserDetail Create page requested");
            return View();
        }

        // POST: UserDetailController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                logger.LogInformation("UserDetail Create submitted");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating UserDetail");
                return View();
            }
        }

        // GET: UserDetailController/Edit/5
        public ActionResult Edit(int id)
        {
            logger.LogInformation($"UserDetail Edit page requested for ID: {id}");
            return View();
        }

        // POST: UserDetailController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                logger.LogInformation($"UserDetail Edit submitted for ID: {id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error editing UserDetail ID: {id}");
                return View();
            }
        }

        // GET: UserDetailController/Delete/5
        public ActionResult Delete(int id)
        {
            logger.LogInformation($"UserDetail Delete page requested for ID: {id}");
            return View();
        }

        // POST: UserDetailController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                logger.LogInformation($"UserDetail Delete confirmed for ID: {id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting UserDetail ID: {id}");
                return View();
            }
        }
    }
}

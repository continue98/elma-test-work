using MvcApplication1.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Microsoft.Office.Interop.Word;
namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult Registration()
        {
            if (Session["session_key"] != null)
                return RedirectToAction("index");
            return View(new Models.RegistrationModel());
        }
        public ActionResult Authorization()
        {
            if (Session["session_key"] != null)
                return RedirectToAction("index");
            return View(new Models.AuthorizationModel());
        }
        [HttpPost]
        public ActionResult Registration(Models.RegistrationModel reg_model)
        {
            using (NHibernate.ISession session = NHibernateSessionManeger.OpenSession())
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }
                else
                {
                    NHibernate.IQuery query = session.CreateQuery("FROM Users WHERE name = '" + reg_model.Name + "'");
                    if (query.List<Models.Users>().Count() > 0)
                    {
                        ViewData["ErrorMessage"] = "    Пользователь с ником " + reg_model.Name + " уже занят   ";
                        return View();
                    }

                    var usr = new Models.Users();
                    usr.Email = reg_model.Email;
                    usr.Name = reg_model.Name;
                    var salt = Guid.NewGuid().ToByteArray();
                    string hashsed_password = Convert.ToBase64String(CryptSharp.Utility.SCrypt.ComputeDerivedKey(System.Text.UTF8Encoding.UTF8.GetBytes(reg_model.Password), salt, 16384, 8, 1, null, 128));
                    usr.HashPassword = hashsed_password;
                    usr.Salt = Convert.ToBase64String(salt);
                    session.Save(usr);

                    return RedirectToAction("authorization");
                }
            }
        }
        [HttpPost]
        public ActionResult Authorization(Models.AuthorizationModel auth_model)
        {
            using (ISession session = NHibernateSessionManeger.OpenSession())
            {
                IQuery query = session.CreateQuery("FROM Users WHERE name = '" + auth_model.Name + "'");

                if (query.List<Users>().Count() == 0)
                {
                    ViewData["ErrorMessage"] = "Неправильный пользователь или пароль";
                    return View();
                }

                Users usr = query.List<Users>()[0];
                string name = usr.Name;
                string hashed_password = usr.HashPassword;
                string salt = usr.Salt;
                int id = usr.Id;
                

                HashUtillity hashUtillty = new HashUtillity();
                if (hashUtillty.compare_password(auth_model.Password, hashed_password, salt))
                {
                    Session.Timeout = 5;
                    Session["session_key"] = name;
                    Session["session_id"] = id;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Неправильный пользователь или пароль";
                    return View();
                }
            }
        }
        [HttpGet]
        public ActionResult LogOut()
        {
            Session["session_key"] = null;
            Session["session_id"] = null;
            return RedirectToAction("index");
        }
        public ActionResult UploadDocs()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadDocs(HttpPostedFileBase file)
        {
            MvcApplication1.src.DownloadHelper upl_file = new MvcApplication1.src.DownloadHelper(Session["session_key"].ToString());
            string error_message = upl_file.UploadUserFile(file);
            if (error_message != null)
            {
                ViewData["ErrorMessage"] = upl_file.ErrorMessage;
            }
            return View();

        }
        public ActionResult MyDocs(string sortOrder)
        {
            if (Session["session_key"] == null)
                return RedirectToAction("Index");

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            using (ISession session = NHibernateSessionManeger.OpenSession())
            {
                var resultList = new List<Docs>();
                var query = session.QueryOver<Docs>();
                resultList = query.Where(x => x.UserId == 5).List().ToList();
                switch (sortOrder)
                {
                    case "name_desc":
                    {
                        resultList = resultList.OrderByDescending(s => s.NameDoc).ToList();
                        break;
                    }
                    case "date_desc":
                    {
                       resultList = resultList.OrderByDescending(s => s.Date).ToList();
                       break;
                    }
                }
                return View(resultList);
            }
        }
        [HttpGet]
        public ActionResult DownloadFile(string path)
        {
            if (Session["session_key"] == null)
                return RedirectToAction("Index");

            string filePath = path;
            string mime_type = "";
            string extensions = System.IO.Path.GetExtension(path).ToLower();

            /* Not good idea. In net framework 4.5 - use 
             *  https://docs.microsoft.com/en-us/dotnet/api/system.web.mimemapping.getmimemapping?redirectedfrom=MSDN&view=netframework-4.7.2#System_Web_MimeMapping_GetMimeMapping_System_String_
             */
            if (extensions == ".pdf")
            {
                mime_type = "application/pdf";
            }
            else if(extensions == ".doc")
            {
                mime_type = "application/msword";
            }
            Response.AddHeader("Content-Disposition", "inline; filename=" + System.IO.Path.GetFileName(filePath));

            return File(filePath, mime_type);
        }
    }
}

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
using NHibernate.Criterion;

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
            if (Session["session_key"] != null)
                return RedirectToAction("index");
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

                var usr = query.List<Users>().ToArray();
                string name = usr[0].Name;
                string hashed_password = usr[0].HashPassword;
                string salt = usr[0].Salt;
                int id = usr[0].Id;
                

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
            if (Session["session_key"] == null)
                return RedirectToAction("index");
            Docs docs = new Docs();
            docs.PathToFile = HttpContext.Server.MapPath(@"~/App_Data/uploads/Users/" + Session["session_key"] + "/");
            docs.NameDoc = System.IO.Path.GetFileName(docs.PathToFile + "/" + file.FileName);

            if(System.IO.File.Exists(docs.PathToFile + docs.NameDoc))
            {
                return new HttpStatusCodeResult(400, "file already exists");
            }
            using (var binaryReader = new System.IO.BinaryReader(Request.Files[0].InputStream))
            {
                docs.BinaryFile = binaryReader.ReadBytes(Request.Files[0].ContentLength);
            }



            if(!System.IO.Directory.Exists(docs.PathToFile))
            {
                System.IO.Directory.CreateDirectory(docs.PathToFile);
            }

            using (var fs = new System.IO.FileStream(docs.PathToFile + docs.NameDoc, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                fs.Write(docs.BinaryFile, 0, docs.BinaryFile.Length);
            }
            using (ISession session = NHibernateSessionManeger.OpenSession())
            {
                session.GetNamedQuery("CreateDocument").
                        SetParameter("UserID", Session["session_id"]).
                        SetParameter("NameDoc", docs.NameDoc).
                        SetParameter("PathToFile", docs.PathToFile).UniqueResult();
            }
            return View();

        }
        // thank for documentation https://docs.microsoft.com/ru-ru/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
        public ActionResult MyDocs(string sortOrder, string searchName, DateTime ? searchDate)
        {
            if (Session["session_key"] == null)
                return RedirectToAction("Index");

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            using (ISession session = NHibernateSessionManeger.OpenSession())
            {
                IList<Docs> resultList = new List<Docs>();

                var query = session.QueryOver<Docs>();
                resultList = query.Where(x => x.UserId == Convert.ToInt64(Session["session_id"])).List();

                if (!String.IsNullOrEmpty(searchDate.ToString()))
                {
                    resultList = query.Where(s => s.Date.Day == searchDate.Value.Day && 
                                                  s.Date.Month == searchDate.Value.Month && 
                                                  s.Date.Year == searchDate.Value.Year).List();
                    ViewBag.SearchDate = searchDate;
                }
                if (!String.IsNullOrEmpty(searchName))
                {
                    resultList = query.Where(s => s.NameDoc.IsLike(searchName, MatchMode.Anywhere)).List();
                    ViewBag.SearchName = searchName;
                }
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
        public ActionResult DownloadFile(string path, string filename)
        {
            if (Session["session_key"] == null)
                return RedirectToAction("Index");


            Docs docs = new Docs();
            docs.PathToFile = path;

            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            using (var binaryReader = new System.IO.BinaryReader(fs))
            {
                docs.BinaryFile = binaryReader.ReadBytes(Convert.ToInt32(binaryReader.BaseStream.Length));
            }

            docs.PathToFile = HttpContext.Server.MapPath(@"~/App_Data/uploads/Users/" + Session["session_key"] + "/");
            docs.NameDoc = System.IO.Path.GetFileName(docs.PathToFile);
            docs.Author = Session["session_key"].ToString();

            string mime_type = "";
            string extensions = System.IO.Path.GetExtension(path).ToLower();

            if (extensions == ".pdf")
            {
                mime_type = "application/pdf";
            }
            else if (extensions == ".doc")
            {
                mime_type = "application/msword";
            }

            Response.BinaryWrite(docs.BinaryFile);
            Response.ContentType = mime_type;
            Response.Headers["Content-Disposition"] = $"inline; filename= " + filename;
            return View();
        }
    }
}

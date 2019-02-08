using MvcApplication1.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                    usr.hash_password = hashsed_password;
                    usr.salt = Convert.ToBase64String(salt);
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
                string hashed_password = usr.hash_password;
                string salt = usr.salt;
                
                HashUtillity hashUtillty = new HashUtillity();
                if (hashUtillty.compare_password(auth_model.Password, hashed_password, salt))
                {
                    Session.Timeout = 10;
                    Session["session_key"] = Session.SessionID;
                    return RedirectToAction("index");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Неправильный пользователь или пароль";
                    return View();
                }
            }
        }
    }
}

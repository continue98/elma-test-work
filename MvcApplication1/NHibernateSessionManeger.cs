using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Cfg;
using NHibernate;
namespace MvcApplication1
{
    public class NHibernateSessionManeger
    {
        public static ISession OpenSession()
        {
            var configuration = new Configuration();
            var configurationPath = HttpContext.Current.Server.MapPath(@"~\Models\hibernate.cfg.xml");
            configuration.Configure(configurationPath);
            var bookConfigurationFile = HttpContext.Current.Server.MapPath(@"~\Mappings\Users.hbm.xml");
            configuration.AddFile(bookConfigurationFile);
            ISessionFactory sessionFactory = configuration.BuildSessionFactory();
            return sessionFactory.OpenSession();
        }
    }
}
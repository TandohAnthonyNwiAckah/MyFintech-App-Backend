namespace TANA.Controller
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using TANA.Data;
    using TANA.Models;

    public class HomeController : BaseController
    {
        private readonly IConfiguration Config;
        private readonly AppDBContext DB;
        private readonly IWebHostEnvironment hostEnvironment;

        public HomeController(
            AppDBContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration Configuration
        )
            : base(dbContext, httpContextAccessor, environment, Configuration)
        {
            DB = dbContext;
            Config = Configuration;
            hostEnvironment = environment;
        }

        public ActionResult index()
        {
            return View();
        }
    }
}

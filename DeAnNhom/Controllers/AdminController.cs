using DeAnNhom.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DeAnNhom.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        private DeAnNhomDatabaseEntities db = new DeAnNhomDatabaseEntities();

        public AdminController()
        { }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager rolesManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = rolesManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }

            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        [OverrideAuthentication]
        public async Task<ActionResult> BecomeAdmin()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (user.Email.ToLower() == "admin@admin.com")
            {
                db.Sellers.Add(new Seller { SellerID = user.Id, ShopName = user.Customer.Name });

                await Task.WhenAll(
                    UserManager.AddToRolesAsync(user.Id, "Admin", "Seller"),
                    db.SaveChangesAsync()
                );

                return RedirectToAction("Index");
            }
            return RedirectToAction("Unauthorization", "Errors");
        }

        public ActionResult CreateCategory()
        {
            Category cate = new Category();
            return View(cate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(Category cate)
        {
            db.Categories.Add(cate);
            db.SaveChanges();

            ViewBag.IsSuccess = true;

            return View(new Category());
        }
    }
}
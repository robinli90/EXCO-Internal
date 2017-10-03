using System.Web;
using System.Web.Optimization;

namespace MvcApplication1
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            #region IncludeDirectory bundles
            bundles.Add(new ScriptBundle("~/admin-lte/js")
                .IncludeDirectory("~/admin-lte/", "*.js", true));

            bundles.Add(new StyleBundle("~/admin-lte/css")
                .IncludeDirectory("~/admin-lte/", "*.css", true));

            bundles.Add(new StyleBundle("~/Content/css")
                .IncludeDirectory("~/Content/", "*.css", true));
            #endregion


            bundles.Add(new Bundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new Bundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new Bundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new Bundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/site.css",
                "~/Content/bootstrap.css",
                "~/admin-lte/css/AdminLTE.css",
                "~/admin-lte/css/skins/skin-black.css",
                "~/admin-lte/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css"
                ));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
            
            bundles.Add(new ScriptBundle("~/admin-lte/js").Include(
                "~/admin-lte/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js",
                "~/admin-lte/js/app.js",
                "~/admin-lte/plugins/fastclick/fastclick.js"

                ));
        }
    }
}
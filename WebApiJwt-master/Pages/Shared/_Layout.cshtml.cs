using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Daewoong.BI
{
    public class _LayoutModel : PageModel
    {
        public Menu menus { get; set; }

        public void OnGet()
        {
            menus = new Menu();
            menus.Category = "A";

            ViewData["Title1"] = "Title";
        }
    }
}
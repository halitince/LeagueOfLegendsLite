using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForm
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            return;
            Bitmap img = new Bitmap(@"D:\OneDrive\lol\lolApp\WebForm\img\harita.1.png");
            Color pixel1 = img.GetPixel(564, 3780);
            Color pixel2 = img.GetPixel(800, 3780);
            Color pixel3 = img.GetPixel(950, 3780);
            Color pixel4 = img.GetPixel(999, 3780);
            var asd = "Durum" + pixel1.A + "_" + pixel2.A + "_" + pixel3.A + "_" + pixel4.A + "_";
            var asda = asd.Length;

        }
    }
}
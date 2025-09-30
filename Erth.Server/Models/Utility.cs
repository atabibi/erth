using System.Globalization;
using System.Text;
using Erth.Shared.Models;

namespace Erth.Server.Models
{
    public static class Utility
    {
        public static string CreateActiveCode(string strSn, CdTypeErth cdTypeErth)
        {
            const ulong BaseCodeForProEdition = 5245689851422347;
            const ulong BaseCodeForStudentEdition = 3335647891306510;
            const ulong BaseCodeForSabtEdition = 1826549123951453;

            char[] space = new char[1] { ' ' };

            ulong sn = 0;
            try
            {

                string str = strSn.Trim(space);
                byte[] bytes = new byte[str.Length];



                Encoding.ASCII.GetBytes(str, 0, str.Length, bytes, 0);


                for (int i = 0; i < bytes.Length; i++)
                    sn = sn * 255 + bytes[i];

                switch (cdTypeErth)
                {
                    case CdTypeErth.Pro:
                        sn = sn - BaseCodeForProEdition;
                        break;
                    case CdTypeErth.Student:
                        sn = sn - BaseCodeForStudentEdition;
                        break;
                    case CdTypeErth.Sabt:
                        sn = sn - BaseCodeForSabtEdition;
                        break;
                }

            }
            catch
            {
                return "خطا در ایجاد کد فعالسازی. لطفا با ۰۹۱۳۲۷۰۸۳۴۱ تماس بگیرید.";
            }

            return sn.ToString();
        }

        public static string ToShamsi(this System.DateTime date)
        {
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date)}/{pc.GetMonth(date)}/{pc.GetDayOfMonth(date)}";
        }
    }

}
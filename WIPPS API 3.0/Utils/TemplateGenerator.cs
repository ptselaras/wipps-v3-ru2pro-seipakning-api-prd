using Microsoft.AspNetCore.Hosting;
using WIPPS_API_3._0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WIPPS_API_3._0.Controllers.FormsController;

namespace WIPPS_API_3._0.Utils
{
    public static class TemplateGenerator
    {
        public static string GetMailReport(IWebHostEnvironment _hostingEnvironment, string base_url, DataReport dataReport, User user)
        {

            var sb = new StringBuilder();

            string urlImage = "";
            string urlLogo = base_url + "/storage/app/public/assets/image/logo.png";

            if (user.RefineryId == 1)
            {
                urlImage = base_url + "/storage/app/public/assets/image/pertamina_ii.png";
            }
            else if(user.RefineryId == 2)
            {
                urlImage = base_url + "/storage/app/public/assets/image/pertamina_iii.png";
            }
            else if (user.RefineryId == 3)
            {
                urlImage = base_url + "/storage/app/public/assets/image/pertamina_iv.png";
            }
            else if (user.RefineryId == 4)
            {
                urlImage = base_url + "/storage/app/public/assets/image/pertamina_v.png";
            }
            else if (user.RefineryId == 5)
            {
                urlImage = base_url + "/storage/app/public/assets/image/pertamina_vi.png";
            }

            sb.Append(@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                        <style>
                            .body{
                                font-family: Helvetica;
                            }
                        </style>
                    </head>
                    <body>
                        
                        <div class='wrapper'>
                            <h1 style='text-align:center; '>WIPPS REPORT</h1>
                            <header>    
            ");

            sb.AppendFormat(@"
                            <div style='text-align:center;'><img src='{0}' style='width:200px;margin-right:200px;' /><img src='{1}' style='width:200px;right:0px;' /></div>" +       
            "", urlImage, urlLogo);
            
            sb.AppendFormat(@"
                            </header>
                            <br>
                            <h4>Data Awal</h4>
                            <table style='text-align:left;right:0px;border-collapse:collapse;width:100%' border='1'>
                                <tr>
                                    <td style='text-align:left;'>Area</td>
                                    <td style='text-align:left;' width='8px'>:</td>
                                    <td style='text-align:left;'>{0}</td>
                                </tr>
                                <tr>
                                    <td style='text-align:left;'>Pekerjaan</td>
                                    <td style='text-align:left;' width='8px'>:</td>
                                    <td style='text-align:left;'>{1}</td>
                                </tr>
                                <tr>
                                    <td style='text-align:left;'>Judul</td>
                                    <td style='text-align:left;' width='8px'>:</td>
                                    <td style='text-align:left;'>{2}</td>
                                </tr>
                                <tr>
                                    <td style='text-align:left;'>Pengguna</td>
                                    <td style='text-align:left;' width='8px'>:</td>
                                    <td style='text-align:left;'>{3}</td>
                                </tr>
                            </table>
                            <br>
                            <br>
            ", dataReport.area_id, dataReport.job_id, dataReport.form_type_id, dataReport.user);

            sb.AppendFormat(@"
                            <h4>Pelaksana Pekerjaan</h4>
                            <table style='text-align:left;right:0px;border-collapse:collapse;width:100%' border='1'>
                                <tr>
                                    <td style='text-align:left;'>Pelaksana 1</td>
                                    <td style='text-align:left;' width='8px'>:</td>
                                    <td style='text-align:left;'>{0} ({1})</td>
                                </tr>
                            </table>
                            <br>
                            <br>
            ", dataReport.companies.Name, dataReport.companies.Type);

            sb.Append(@"
                            <h4>Isi Formulir</h4>
                            <table style='text-align:left;right:0px;border-collapse:collapse;width:100%' border='1'>
                                <tr>
                                    <td style='text-align:left;'><b>Komponen</b></td>
                                    <td style='text-align:left;' width='8px'><b>Catatan</b></td>
                                    <td style='text-align:left;'><b>Gambar</b></td>
                                    <td style='text-align:left;'><b>Status</b></td>
                                </tr>
            ");

            foreach(var value in dataReport.components)
            {
                string status = "";
                if (value.status == 1)
                {
                    status = "Safe";
                } else if (value.status == 2)
                {
                    status = "Unsafe";
                } else if (value.status == 3)
                {
                    status = "N/A";
                }

                sb.AppendFormat(@"
                                <tr>
                                    <td style='text-align:left;'>{0}</td>
                                    <td style='text-align:left;' width='8px'>{1}</td>
                                    <td style='text-align:left;'>
                ", value.text, value.note);

                foreach(var val in value.images)
                {
                    string img = val.File;
                    sb.AppendFormat(@"
                                        <img src='{0}' style='width:100px;'>
                    ", img);
                }

                sb.AppendFormat(@"
                                    </td>
                                    <td style='text-align:left;'>{0}</td>
                                </tr>
                ", status);
            }

            sb.Append(@"
                            </table>
                        </div>
                    </body>
                </html>
            ");

            return sb.ToString();
        }

        public static string GetMailMessage(string base_url)
        {
            string url = base_url + "/storage/app/public/assets/image/logo.png";

            var sb = new StringBuilder();

            sb.Append(@"
                <!DOCTYPE html>
                <html>
                    <head>
                        <meta charset='utf-8' />
                        <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                        <meta name='viewport' content='width=device-width, initial-scale=1' />
                    </head>
                    <body>
            ");

            sb.Append(@"<img src=""cid:{0}"" style='width:400px;' />");

            sb.Append(@"
                        <br>
                        WIPPS Report
                        <br>
                        Laporan ini di generate dan di kirim oleh sistem.
                        <br>
                        Pesan tidak perlu di balas.
                    </body>
                </html>
            ");

            return sb.ToString();
        }
    }
}

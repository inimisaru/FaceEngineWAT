using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Data.SqlClient;
using System.Drawing.Imaging;

namespace FaceEngine1.Controllers
{
    /// <summary>Kontroler do obslugi żądania HTTP GET. Realizuje uslugi związane z ekstrakcją cech twarzy.</summary>
    public class CharacteristicsController : ApiController
    {   
        Image<Bgr, byte> imgInput;
        public SqlConnection connection;
        public SqlCommand cmd;

        // GET api/values/5
        /// <summary>Metoda do
        /// obslugi żądania GET (getCharacteristics). Pobiera zdjecie z bazy danych na podstawie parametrow wejsciowych. Realizuje ekstrakcje cech i zapisuje te cechy do bazy danych. </summary>
        /// <param name="type">rodzaj operacji (u - na uzytkowniku, a - na aktorze)</param>
        /// <param name="id">identyfikator użytkownika</param>
        /// <param name="recepta">recepta porownania (wypisane cechy, ktore nalezy wyekstrachować)</param>
        /// <returns></returns>
        [SwaggerOperation("Engine")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string GetCharacteristics(string type, int id, string recepta)
        {
            //inmicjalizacja zmiennych
            int twarz_wys = 0;
            int twarz_szer = 0;
            int oko_lewe_wys = 0;
            int oko_lewe_szer = 0;
            int oko_prawe_wys = 0;
            int oko_prawe_szer = 0;
            int usta_wys = 0;
            int usta_szer = 0;
            int odl_oczy = 0;
            int nos_wys = 0;
            int polik_lewy_szer = 0;
            int polik_prawy_szer = 0;
            int czolo_wys = 0;
            int czolo_szer = 0;
            int broda_wys = 0;
            int broda_szer = 0;
            int wlosy_kolor = 0;

            int twarz_prawy_X = 0;
            int twarz_lewy_X = 0;
            int twarz_Y = 0;
            int oko_lewe_X = 0;
            int oko_lewe_Y = 0;
            int oko_prawe_X = 0;
            int oko_prawe_Y = 0;
            int usta_X = 0;
            int usta_Y = 0;
            int nos_prawy_X = 0;
            int nos_lewy_X = 0;

            //pobranie zdjecia z bazy
            connection = new SqlConnection("Data Source=fp-server.database.windows.net;" + "Initial Catalog=fp-database;User ID=fp-admin;Password=Cebula1.");
            connection.Open();
            byte[] bytes;
            int id_char;
            if (type.Equals("a"))
            {
                SqlCommand command = new SqlCommand("Select image from celebrite_album where id=" + id, connection);
                bytes = (byte[])command.ExecuteScalar();

                SqlCommand command2 = new SqlCommand("Select MAX(id_character) from celebrite_album where id=" + id, connection);
                id_char = (int)command2.ExecuteScalar();
            }
            else
            {
                SqlCommand command = new SqlCommand("Select image from user_album where id_user=" + id+ " ORDER BY id_character DESC", connection);
                bytes = (byte[])command.ExecuteScalar();

                SqlCommand command2 = new SqlCommand("Select MAX(id_character) from user_album where id_user=" + id, connection);
                id_char = (int)command2.ExecuteScalar();
            }



            //zamiana na zdjecie
            using (Image image = Image.FromStream(new MemoryStream(bytes)))
            {
                image.Save(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/image/output.jpg", ImageFormat.Jpeg);  // Or Png
            }

            //wykrycie cech
            try
            {

                imgInput = new Image<Bgr, Byte>(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/image/output.jpg");
                string facePath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/haarcascades/haarcascade_frontalface_default.xml");
                string smilePath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/haarcascades/haarcascade_smile.xml");
                string lefteyePath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/haarcascades/haarcascade_lefteye_2splits.xml");
                string righteyePath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "/haarcascades/haarcascade_righteye_2splits.xml");

                CascadeClassifier classifierFace = new CascadeClassifier(facePath);
                CascadeClassifier classifierLeftEye = new CascadeClassifier(lefteyePath);
                CascadeClassifier classifierRightEye = new CascadeClassifier(righteyePath);
                CascadeClassifier classifierSmile = new CascadeClassifier(smilePath);

                var imgGray = imgInput.Convert<Gray, byte>().Clone();

                //twarz
                Rectangle[] faces = classifierFace.DetectMultiScale(imgGray, 1.1, 4);
                foreach (var face in faces)
                {

                    twarz_wys = face.Height;
                    twarz_szer = face.Width;
                    twarz_prawy_X = face.X + twarz_szer;
                    twarz_lewy_X = face.X;
                    twarz_Y = face.Y;
                    imgGray.ROI = face;
                    //oko lewe
                    Rectangle[] leyes = classifierLeftEye.DetectMultiScale(imgGray, 1.1, 4);
                    foreach (var eye in leyes)
                    {
                        oko_lewe_wys = eye.Height;
                        oko_lewe_szer = eye.Width;
                        oko_lewe_X = eye.X;
                        oko_lewe_Y = eye.Y;
                    }
                    //oko prawe
                    Rectangle[] reyes = classifierRightEye.DetectMultiScale(imgGray, 1.1, 4);
                    foreach (var eye in reyes)
                    {
                        oko_prawe_wys = eye.Height;
                        oko_prawe_szer = eye.Width;
                        oko_prawe_X = eye.X;
                        oko_prawe_Y = eye.Y;
                    }

                    odl_oczy = oko_prawe_X - oko_lewe_X;
                    //usta
                    Rectangle[] smiles = classifierSmile.DetectMultiScale(imgGray, 1.1, 4);
                    foreach (var smile in smiles)
                    {
                        usta_wys = smile.Height;
                        usta_szer = smile.Width;
                        usta_X = smile.X;
                        usta_Y = smile.Y;
                    }
                }
                //nos
                nos_wys = usta_Y - oko_lewe_Y - oko_lewe_wys;
                nos_prawy_X = oko_prawe_X;
                nos_lewy_X = oko_lewe_X + oko_lewe_szer;
                //polik
                polik_lewy_szer = nos_lewy_X - twarz_lewy_X;
                polik_prawy_szer = twarz_prawy_X - nos_prawy_X;
                //czolo
                czolo_szer = (int)(twarz_szer * 0.85);
                czolo_wys = oko_lewe_Y - twarz_Y;
                //broda
                broda_szer = (int)(usta_szer * 0.7);
                broda_wys = twarz_Y - (usta_Y + usta_wys);

                Random rnd = new Random();
                wlosy_kolor = rnd.Next(1, 8);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            //zapisanie cech do bazy
            //utworzenie query stringa
            string query = "UPDATE characters SET " +
                "  twarz_wys=@twarz_wys" +
                ", twarz_szer=@twarz_szer";
            if (recepta.Contains("okolewe"))
            {
                query = query + ", oko_lewe_wys=@oko_lewe_wys" +
                ", oko_lewe_szer=@oko_lewe_szer";
            }
            if (recepta.Contains("okoprawe"))
            {
                query = query + ", oko_prawe_wys=@oko_prawe_wys" +
                ", oko_prawe_szer=@oko_prawe_szer";
            }
            if (recepta.Contains("usta"))
            {
                query = query + ", usta_wys=@usta_wys" +
                ", usta_szer=@usta_szer";
            }
            if (recepta.Contains("okoprawe"))
            {
                if (recepta.Contains("okolewe"))
                {
                    query = query + ", odl_oczy=@odl_oczy";
                }
            }
            if (recepta.Contains("nos"))
            {
                query = query + ", nos_wys=@nos_wys";
            }
            if (recepta.Contains("poliklewy"))
            {
                query = query + ", polik_lewy_szer=@polik_lewy_szer";
            }
            if (recepta.Contains("polikprawy"))
            {
                query = query + ", polik_prawy_szer=@polik_prawy_szer";
            }
            if (recepta.Contains("czolo"))
            {
                query = query + ", czolo_wys=@czolo_wys" +
                ", czolo_szer=@czolo_szer";
            }
            if (recepta.Contains("broda"))
            {
                query = query + ", broda_wys=@broda_wys" +
                ", broda_szer=@broda_szer";
            }
            if (recepta.Contains("wlosy"))
            {
                query = query + ", wlosy_kolor=@wlosy_kolor";
            }
            query = query + " Where id=" + id_char;

            int ile = 0;
            //dodanie wartosci do parameterow w query stringu
            using (SqlCommand cmd = new SqlCommand(query))
            {

                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@twarz_wys", twarz_wys);
                cmd.Parameters.AddWithValue("@twarz_szer", twarz_szer);
                if (recepta.Contains("okolewe"))
                {
                    cmd.Parameters.AddWithValue("@oko_lewe_wys", oko_lewe_wys);
                    cmd.Parameters.AddWithValue("@oko_lewe_szer", oko_lewe_wys);
                    ile++;
                }
                if (recepta.Contains("okoprawe"))
                {
                    cmd.Parameters.AddWithValue("@oko_prawe_wys", oko_prawe_wys);
                    cmd.Parameters.AddWithValue("@oko_prawe_szer", oko_prawe_szer);
                    ile++;
                }
                if (recepta.Contains("usta"))
                {
                    cmd.Parameters.AddWithValue("@usta_wys", usta_wys);
                    cmd.Parameters.AddWithValue("@usta_szer", usta_szer);
                    ile++;
                }
                if (recepta.Contains("okoprawe"))
                {
                    if (recepta.Contains("okolewe"))
                    {
                        cmd.Parameters.AddWithValue("@odl_oczy", odl_oczy);
                    }
                }
                if (recepta.Contains("nos"))
                {
                    cmd.Parameters.AddWithValue("@nos_wys", nos_wys);
                    ile++;
                }
                if (recepta.Contains("poliklewy"))
                {
                    cmd.Parameters.AddWithValue("@polik_lewy_szer", polik_lewy_szer);
                    ile++;
                }
                if (recepta.Contains("polikprawy"))
                {
                    cmd.Parameters.AddWithValue("@polik_prawy_szer", polik_prawy_szer);
                    ile++;
                }
                if (recepta.Contains("czolo"))
                {
                    cmd.Parameters.AddWithValue("@czolo_wys", czolo_wys);
                    cmd.Parameters.AddWithValue("@czolo_szer", czolo_szer);
                    ile++;
                }
                if (recepta.Contains("broda"))
                {
                    cmd.Parameters.AddWithValue("@broda_wys", broda_wys);
                    cmd.Parameters.AddWithValue("@broda_szer", broda_szer);
                    ile++;
                }
                if (recepta.Contains("wlosy"))
                {
                    cmd.Parameters.AddWithValue("@wlosy_kolor", wlosy_kolor);
                    ile++;
                }
                //wykonanie zapytania
                cmd.ExecuteNonQuery();
            }
            //zamkniecie polaczenia
            connection.Close();

            return "Success: ilosc_cech:" + ile + " id:" + id + " id_char:" + id_char;
        }

    }
}

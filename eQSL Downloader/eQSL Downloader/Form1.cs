using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Collections.Specialized;




namespace eQSL_Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.anchor.Trim().Length > 0 )
            {
                textBox1.Text = BinaryToString(Properties.Settings.Default.anchor);
                ckSaveCR.Checked = true;
            }
            if (Properties.Settings.Default.keyhole.Trim().Length > 0)
            {
                textBox2.Text = BinaryToString(Properties.Settings.Default.keyhole);
                ckSaveCR.Checked = true;
            }

            try
            {
                textBox3.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\IEN\\eQSL"; 
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                textBox3.Text = "c:\\eQSL";
            }
            if (System.IO.Directory.Exists(textBox3.Text))
            {
                int c = System.IO.Directory.GetFiles(textBox3.Text).Count();
                toolStripStatusLabel1.Text = "Cards downloaded: " + c.ToString();

            }
            //check for saved username and password
            

        }
        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }
        
        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }
            return Encoding.ASCII.GetString(byteList.ToArray());
        }


        void saveUser()
        {
            if (ckSaveCR.Checked)
            {
                string s = textBox2.Text;
                Properties.Settings.Default.anchor = StringToBinary(textBox1.Text);
                Properties.Settings.Default.keyhole = StringToBinary(textBox2.Text);
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.anchor = ""; 
                Properties.Settings.Default.keyhole = ""; 
                Properties.Settings.Default.Save();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                saveUser();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            
            login();
            if (System.IO.Directory.Exists(textBox3.Text))
            {
                int c = System.IO.Directory.GetFiles(textBox3.Text).Count();
                toolStripStatusLabel1.Text = "Cards downloaded: " +
                 c.ToString();

            }

        }

        void login()
        {
            var client = new CookieAwareWebClient();
            client.Encoding = Encoding.UTF8;

            // Post values

            var values = new NameValueCollection();
            values.Add("Callsign", this.textBox1.Text);
            values.Add("EnteredPassword", this.textBox2.Text);
            values.Add("Login", "Go");   //The button

            // Logging in
            client.UploadValues("http://www.eqsl.cc/qslcard/LoginFinish.cfm", values); // You may verify the result. It works with https :)

            var html = "";
           
            // Download some secret page
            if (checkBox1.Checked)
            {
                html = client.DownloadString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=1&Reject=0");
            } else
            {
                html = client.DownloadString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=0&Reject=0");
            }

            if (html.IndexOf(@"eQSLs more than can be displayed on this screen") > -1)
            {
                System.Windows.Forms.MessageBox.Show(@"You have too many cards.  You will need to wait for an updated version!");
                return;
            }
           
            //Console.Write(html);
            //Form2 f2 = new Form2();
            //f2.textBox1.Text = html;

            //check for debug
            //f2.Show();
            int i = 0;
            string mylist = parseDisplay(html);
            foreach (string s in mylist.Split('|'))
            {
                if (s.Trim().Length == 0)
                {
                    continue;

                }
                var h = client.DownloadString("http://www.eqsl.cc/qslcard/" + s);

                string s1 = s.Substring(s.IndexOf("Callsign=") + 9);
                string callsign = s1.Substring(0, s1.IndexOf("&"));

                //System.Console.WriteLine(h);
                h = h.Substring(h.IndexOf("img src="));
                h = h.Substring(h.IndexOf("\"")+1);
                h = h.Substring(0,h.IndexOf("\""));

                if (!(System.IO.Directory.Exists(textBox3.Text)))
                {
                    System.IO.Directory.CreateDirectory(textBox3.Text);
                }

                string filename = System.IO.Path.Combine(textBox3.Text , callsign + "-" +  DateTime.Now.ToString("yyMMddmmssff") +  ".png");
                //while (System.IO.File.Exists(filename))
                //{ 
                    
                //}

                client.DownloadFile("http://www.eqsl.cc" + h, filename);
                i = i + 1;
                //<CENTER>
                //<img src="/CFFileServlet/_cf_image/_cfimg-632732702018634097.PNG" alt="" />
                      //get call sign 
                      // download page
                      // parse image name 
                      // download image 
                      // save image as call sign...  if exists.. _1,_2 etc... 
            }
            System.Windows.Forms.MessageBox.Show("Download of "  + i.ToString() + " QSL cards Complete");
        }
        string parseDisplay(string h)
        {
            string newString = "";

            int i = h.IndexOf("DisplayeQSL.cfm");
            

            while( i > -1)
            {
                h = h.Substring(i);
                string urlString = h.Substring(0,h.IndexOf("'"));
                h = h.Substring(15); // cut out current displayeQSL
                i = h.IndexOf("DisplayeQSL.cfm");
                newString = newString + urlString  + "|";
            }

            //foreach (string s in h.Split(new string[] {@"DisplayeQSL.cfm?Callsign"}, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    newString = newString + "||";
            //}

            return newString;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://itunes.apple.com/us/app/learning-morse-code/id735785166?mt=8");
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.ToString());
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.indianaelmernetwork.us");
            }
            catch (Exception ex)
            {
                System.Console.Write(ex.ToString());
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(textBox3.Text); 
            }catch (Exception ex)
            {
            
            }
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.Show(this); 

        }

    }

    public class CookieAwareWebClient : WebClient
    {
        private CookieContainer cookieContainer = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = cookieContainer;
            }
            return request;
        }
    }
}

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
        bool ParseTest = false;
        string g_filename = "";


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
            dateTimePicker1.Value = new DateTime(1900,01,01);
            dateTimePicker2.Value = DateTime.Now;
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


            var listofbadcards = "";
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

                string myyear = dateTimePicker1.Value.ToString("yyyy");
                string mymonth = dateTimePicker1.Value.ToString("MM");
                string hMyYear = dateTimePicker2.Value.ToString("yyyy");
                string hMyMonth = dateTimePicker2.Value.ToString("MM");



                // Download some secret page
                if (checkBox1.Checked)
                {
                    html = client.DownloadString(Uri.EscapeUriString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=1&Reject=0&LimitDateLo=" + myyear + " " + mymonth + "&LimitDateHi=" + hMyYear + " " + hMyMonth));
                }
                else
                {
                    html = client.DownloadString(Uri.EscapeUriString("http://www.eqsl.cc/qslcard/InBox.cfm?Archive=0&Reject=0&LimitDateLo=" + myyear + " " + mymonth + "&LimitDateHi=" + hMyYear + " " + hMyMonth));
                }

                if (html.IndexOf(@"eQSLs more than can be displayed on this screen") > -1)
                {
                    System.Windows.Forms.MessageBox.Show(@"You have too many cards.  You will need to change the date to filter the amount of cards!");
                    return;
                }
            
            //check for debug
            //f2.Show();
            int i = 0;
            string mylist = parseDisplay(html);
            string destinationFileName = "";
            foreach (string s in mylist.Split('|'))
            {
                int error_idx_fe = 0;
                try
                {
                    if (s.Trim().Length == 0)
                    {
                        continue;

                    }
                    error_idx_fe = 10;
                    //check to see if file already exists. 
                    destinationFileName = s.Substring(s.IndexOf("?") + 1);
                    error_idx_fe = 20;
                    destinationFileName = Uri.UnescapeDataString(destinationFileName);
                    error_idx_fe = 30;
                    destinationFileName = destinationFileName.Replace('&', '_');
                    error_idx_fe = 40;
                    destinationFileName = destinationFileName.Replace(' ', '_');
                    error_idx_fe = 50;
                    destinationFileName = destinationFileName.Replace('.', '_');
                    error_idx_fe = 60;
                    destinationFileName = destinationFileName.Replace(';', '_');
                    error_idx_fe = 70;
                    destinationFileName = destinationFileName.Replace(':', '_');
                    error_idx_fe = 80;
                    destinationFileName = destinationFileName.Replace('?', '_');
                    destinationFileName = destinationFileName.Replace('@', '_');
                    destinationFileName = destinationFileName.Replace('^', '_');
                    destinationFileName = destinationFileName.Replace('/', '-');

                    error_idx_fe = 120;
                    destinationFileName = System.IO.Path.Combine(textBox3.Text, destinationFileName);



                    destinationFileName = destinationFileName + ".png";

                    try
                    {
                        if (System.IO.File.Exists(destinationFileName))
                        {
                            System.IO.FileInfo fi = new System.IO.FileInfo(destinationFileName);

                            if (fi.Length > 0)
                            {
                                continue;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    var h = client.DownloadString("http://www.eqsl.cc/qslcard/" + s);
                    error_idx_fe = 160;

                    string s1 = s.Substring(s.IndexOf("Callsign=") + 9);
                    error_idx_fe = 210;
                    string callsign = s1.Substring(0, s1.IndexOf("&"));
                    error_idx_fe = 220;
                    if (h.IndexOf("ERROR: eQSL Graphic not available") > -1)
                    {
                        listofbadcards = listofbadcards + callsign + ",";
                        continue;
                    }
                    if (h.IndexOf("ERROR - Too many queries overloading the system. Slow down!") > -1)
                    {

                        System.Threading.Thread.Sleep(5000);
                        h = client.DownloadString("http://www.eqsl.cc/qslcard/" + s);

                        //continue;
                    }

                    //one last final check.. 
                    if (h.IndexOf("ERROR") > -1)
                    {
                        MessageBox.Show("An error happened that was not expected(Send screen shot to ac9hp@arrl.net):\r\n" + h.ToString());
                        continue;
                    }

                    error_idx_fe = 510;
                    //System.Console.WriteLine(h);
                    try
                    {
                        h = h.Substring(h.IndexOf("img src="));
                    }
                    catch (Exception imgexp)
                    {
                        // missing image ..  skip it
                        listofbadcards = listofbadcards + callsign + ",";
                        continue;
                    }
                        error_idx_fe = 710;
                    h = h.Substring(h.IndexOf("\"") + 1);
                    error_idx_fe = 720;
                    h = h.Substring(0, h.IndexOf("\""));
                    error_idx_fe = 730;
                    if (!(System.IO.Directory.Exists(textBox3.Text)))
                    {
                        System.IO.Directory.CreateDirectory(textBox3.Text);
                    }

                    string filename = System.IO.Path.Combine(textBox3.Text, callsign + "-" + DateTime.Now.ToString("yyMMddmmssff") + ".png");
                    //while (System.IO.File.Exists(filename))
                    //{ 

                    //}

                    client.DownloadFile("http://www.eqsl.cc" + h, destinationFileName);
                    i = i + 1;
                    //10 seconds as per API guide
                    System.Threading.Thread.Sleep(10000);
                    //<CENTER>
                    //<img src="/CFFileServlet/_cf_image/_cfimg-632732702018634097.PNG" alt="" />
                    //get call sign 
                    // download page
                    // parse image name 
                    // download image 
                    // save image as call sign...  if exists.. _1,_2 etc...
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error processing has occurred:" + ex.Message);
                    MessageBox.Show("Debug information: " + s);
                    MessageBox.Show("Debug Number: " + error_idx_fe.ToString());

                }
            }
            System.Windows.Forms.MessageBox.Show("Download of "  + i.ToString() + " QSL cards Complete");
            if (listofbadcards.Trim().Length > 0 )
            {
                MessageBox.Show("Unable to download cards for the following call signs due to bad cards:\r\n" + listofbadcards);
            }
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

        private void testADIFFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "ADIF files (*.adi)|*.adi";
            ofd.DefaultExt= "ADI";
            ofd.Title = "Select ADFI file to process";
            ofd.ShowDialog(this);

            String adif_file = ofd.FileName;

            if (adif_file.Trim().Length == 0)
            {
                return;
            }
            ParseTest = true;
            g_filename = adif_file;


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

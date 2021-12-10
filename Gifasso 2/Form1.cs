//This code was written by Kevin Gragg of iLL-Logic Studios
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using GifMotion;

namespace Gifasso_2
{
    public partial class Form1 : Form
    {
        //variables
        private Image img;
        private bool sizeUp = false;
        private int checker = 0;
        private int distance = 20;
        private int T2 = 0;
        private FrameDimension dimension;
        private bool frameLook = false;        
        private int iTick = 0;
        private bool pane1 = true;
        private int fCount = 0;
        private Font myfont;
        private Color textColor = Color.Black;
        private Color bgColor = Color.White;
        private bool pause = true;
        private static Color colorAtm;
        private string fontLocation;

        //constants
        private const string BLACK = "@";
        private const string CHARCOAL = "#";
        private const string DARKGRAY = "8";
        private const string MEDIUMGRAY = "&";
        private const string MEDIUM = "o";
        private const string GRAY = ":";
        private const string SLATEGRAY = "*";
        private const string LIGHTGRAY = ".";
        private const string WHITE = " ";
        
        //Lists
        private List<string> Frames = new List<string>();//ascii text
        private List<Bitmap> imageFramesReg = new List<Bitmap>();//images
        private List<Bitmap> imageFrames = new List<Bitmap>();//images that resize
        private List<Bitmap> AsciiFrames = new List<Bitmap>();//ascii text images
        private List<string> fonts = new List<string>();

        public Form1()
        {
            InitializeComponent();
            initialize();
        }

        private void initialize()
        {
            //set font
            myfont = new Font(new FontFamily("Consolas"), 8.0f);
            Font example = new Font(myfont.Name, 12f);
            toolStripLabel4.Font = example;
            //initialize pictureboxes sizemode
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            //set image in resources as variable for easy management
            img = Properties.Resources.magixm;

            //generate list of fonts for combobox from computer
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fonts.Add(font.Name);//add font to list              
                toolStripComboBox1.Items.Add(font.Name);//add to combobox                
            }
            toolStripComboBox1.SelectedItem = textBox3.Font.Name; //set combobox to textbox3's font
            splitContainer2.Panel2Collapsed = true; //collapse panel2 so text ascii is displayed on startup
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "200";
            textBox2.Text = "100";
            getFrames();//get frames for resources image
            //set picturebox to that image to avoid null referencing (annoying)
            pictureBox1.Image = imageFramesReg[0];//set to first frame
            pictureBox2.Image = AsciiFrames[0];//set to first frame
            textBox3.Text = Frames[0];//set to first frame
            toolStripLabel3.Text = $"Frame:0{0 + 1}";
            dimensions();//update label4 with image size
        }

        private void dimensions()
        {
            label4.Text = $"{img.Width}x{img.Height} Actual Size"; //update label4 with image size        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //show file dialog
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //open image from file and set to img var then set picturebox image to that           
            img = Image.FromFile(openFileDialog1.FileName);
            //handle character sizing upon first load of image to prevent accidental overhaul when going from larger image to smaller for example
            if(img.Width > 500 || img.Height > 500)
            {
                textBox1.Text = "200";
                textBox2.Text = "100";
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                textBox1.Text = Convert.ToString(img.Width);
                textBox2.Text = Convert.ToString(img.Height);
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }

            getFrames();//run getFrames method            
            pictureBox1.Image = imageFramesReg[0];//set pbox1 to first frame of reg images
            pictureBox2.Image = AsciiFrames[0];//set pbox2 to first frame of ascii images
            textBox3.Text = Frames[0];//set tb3 to first string of frames

            dimensions();//update image size in label
            timer1.Start();//run pane sizing timer to adjust pane if need be, if not, timer will immediately turn off 
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //keep picturebox height and width the same
            pictureBox1.Height = pictureBox1.Width;

            //this.Text = $"{pictureBox1.Height} {pictureBox1.Width}";

            //Autotune picturebox sizemode for experience
            if(img.Width > pictureBox1.Width || img.Height > pictureBox1.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox1.Height = img.Height;
            }
           
            //fixes issue where picturebox size gets reduced and picturebox goes off screen
            if(pictureBox1.Width < 150 || pictureBox1.Height < 150)
            {
                pictureBox1.Width = 150;
                pictureBox1.Height = 150;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //expand or contract view of image from thumbnail to actual size
            if (sizeUp)
            {
                sizeUp = false;
                button3.Text = "Expand View >";
            }
            else
            {
                if (img.Width < 150 || img.Height < 150)
                {
                    timer2.Start();
                }
                else
                {
                    sizeUp = true;
                    button3.Text = "Contract View <";
                }

            }            
            timer1.Start();          
        }

        private void timer1_Tick(object sender, EventArgs e)
        {            
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = false;
                Invoke(new Action(() =>
                {                

                    //handles automation
                    if (img.Width < 150 || img.Height < 150)
                    {
                        sizeUp = false;
                        button3.Text = "Expand View >";
                        if (splitContainer1.SplitterDistance != 200)
                        {
                            splitContainer1.SplitterDistance -= distance;                    
                        }
                        else
                        {
                            pictureBox1.Width = 150;
                            pictureBox1.Height = 150;
                            timer1.Stop();                   
                        }
                    }
                    else
                    {
                        if(sizeUp)
                        {           
                            if(pictureBox1.Width < img.Width)
                            {
                                splitContainer1.SplitterDistance += distance;
                            }
                            else if(pictureBox1.Width > img.Width)
                            {                        
                                splitContainer1.SplitterDistance -= distance;
                                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                                checker += 1;
                                if(checker > 0)
                                {
                                    timer1.Stop();
                                    pictureBox1.Size = img.Size;
                                    checker = 0;
                                }
                            }               
                            else
                            {                    
                                timer1.Stop();
                            }              
                        }
                        else
                        {
                            if(splitContainer1.SplitterDistance != 200)
                            {
                                splitContainer1.SplitterDistance -= distance;
                            }
                            else
                            {
                                pictureBox1.Width = 150;
                                pictureBox1.Height = 150;                    
                                timer1.Stop();                    
                            }
                        }
                    }
                }));

            }).Start();      
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(e.ClickedItem.Text == "About..")
            {
                //About Gifasso
                MessageBox.Show("Gifasso was created by iLL-Logic Studios 2022 All Rights Reserved.");
            }           
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //'already full size' toast
            label3.Visible = true;
            T2 += 1;            
            if(T2 >= 200)
            {
                label3.Visible = false;
                T2 = 0;
                timer2.Stop();
            }                 
        }

        public static string GrayscaleImageToASCII(Image img)
        {
            StringBuilder html = new StringBuilder();
            Bitmap bmp = new Bitmap(img);
            
            try
            {              
                for (int y = 0; y <= bmp.Height - 1; y++)
                {
                    for (int x = 0; x <= bmp.Width - 1; x++)
                    {
                        Color col = bmp.GetPixel(x, y);
                        colorAtm = col;
                        //col = Color.FromArgb((col.R + col.G + col.B) / 3, (col.R + col.G + col.B) / 3, (col.R + col.G + col.B) / 3)
                        int rValue = int.Parse(col.R.ToString());
                        html.Append(getGrayShade(rValue));
                        
                        if (x == bmp.Width - 1)
                        {
                            html.Append("\n");
                        }                            
                    }
                }
 
                return html.ToString();                
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
            finally
            {
                bmp.Dispose();
            }
        }

        private static string getGrayShade(int redValue)
        {
            string asciival;

            if (redValue >= 230)
                asciival = WHITE;
            else if (redValue >= 200)
                asciival = LIGHTGRAY;
            else if (redValue >= 180)
                asciival = SLATEGRAY;
            else if (redValue >= 160)
                asciival = GRAY;
            else if (redValue >= 130)
                asciival = MEDIUM;
            else if (redValue >= 100)
                asciival = MEDIUMGRAY;
            else if (redValue >= 70)
                asciival = DARKGRAY;
            else if (redValue >= 50)
                asciival = CHARCOAL;
            else
                asciival = BLACK;

            return asciival;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Gifasso!
            if(timer3.Enabled)
            {
                timer3.Stop();
                button2.Text = "Gifasso!";
                button4.Enabled = true;
                pause = true;

            }
            else
            {
                timer3.Start();
                button2.Text = "Stop!";
                button4.Enabled = false;
                pause = false;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        { 
            if (iTick > Frames.Count - 1)
            {
                iTick = 0;
            }
            else
            {                
                if(pane1)
                {
                    textBox3.Text = Frames[iTick];
                }
                else
                {
                    pictureBox2.Image = AsciiFrames[iTick];
                }     
                pictureBox1.Image = imageFramesReg[iTick];

                if(iTick < 9)
                {
                    toolStripLabel3.Text = $"Frame:0{iTick + 1}";
                }
                else
                {
                    toolStripLabel3.Text = $"Frame:{iTick + 1}";
                }
                
                iTick += 1;
            }             
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //clear image           
            img = Properties.Resources.magixm; //set picturebox image to default pic
            textBox1.Text = "200";
            textBox2.Text = "100";
            getFrames();
            pictureBox1.Image = imageFramesReg[0];
            pictureBox2.Image = AsciiFrames[0];
            textBox3.Text = Frames[0];
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            dimensions();            
            timer1.Start();
        }

        private void getFrames()
        {
            dimension = new FrameDimension(img.FrameDimensionsList[0]);
            // Number of frames
            int frameCount = img.GetFrameCount(dimension);//get framecount
            listBox1.Items.Clear();//clear listbox
            Frames.Clear();//clear ascii text frames list
            imageFrames.Clear();//clear gif image frames list that can be resized
            imageFramesReg.Clear();//clear gif image frames list
            AsciiFrames.Clear();//clear ascii art image frames list           

            for (int i = 0; i < img.GetFrameCount(dimension); i++)//for each frame
            {
                listBox1.Items.Add($"Frame: {i+1}");//add indexes of frames to listbox
                img.SelectActiveFrame(dimension, i);//not quite sure
                Bitmap bmpReg = new Bitmap(img);
                imageFramesReg.Add(bmpReg);
                Bitmap bmp = new Bitmap(img, Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox2.Text));//create bitmap of current frame
                imageFrames.Add(bmp);//add bitmap of frame to image frames list
                Frames.Add(GrayscaleImageToASCII(bmp));//do the ascii conversion of each frame and add to ascii text frames list < this also takes from the resizable gif image list so ascii art can be resized                
                Bitmap asciiconv = new Bitmap(DrawText(Frames[i], myfont, textColor, bgColor));
                AsciiFrames.Add(asciiconv);   
            }
        }  

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //change font of Ascii Textbox
            myfont = new Font($"{toolStripComboBox1.SelectedItem}", 8.0f);
            Font example = new Font(myfont.Name, 12f);
            toolStripLabel4.Font = example;
            textBox3.Font = myfont;            
            getFrames();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //pause animation and view selected frame
            frameLook = true;
            if(frameLook)
            {
                pictureBox1.Image = null;            
                pictureBox1.Image = imageFramesReg[listBox1.SelectedIndex];
                pictureBox2.Image = null;
                pictureBox2.Image = AsciiFrames[listBox1.SelectedIndex];
                img.SelectActiveFrame(dimension, Convert.ToInt32(listBox1.SelectedIndex));
                timer3.Stop();
                textBox3.Text = Frames[listBox1.SelectedIndex];

                if(listBox1.SelectedIndex < 9)
                {
                    toolStripLabel3.Text = $"Frame:0{listBox1.SelectedIndex + 1}";
                }
                else
                {
                    toolStripLabel3.Text = $"Frame:{listBox1.SelectedIndex + 1}";
                }                
            }          
        }

        private void listBox1_MouseLeave(object sender, EventArgs e)
        {
            //resume playing animation     
            if (!(pause))
            {
                timer3.Start();     
            }          
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //handles behaviors and automations when toggling between 'Ascii Text' and 'Ascii Art' views
            if(pane1)
            {
                pane1 = false;
                toolStripButton1.Text = "Ascii Art";
                splitContainer2.Panel1Collapsed = true;
                toolStripLabel2.Text = "Now Viewing: ASCii Art (bitmap)";
            }
            else
            {
                pane1 = true;
                toolStripButton1.Text = "Ascii Text";
                splitContainer2.Panel2Collapsed = true;
                toolStripLabel2.Text = "Now Viewing: ASCii Text (string)";
            }            
        }

        private void pictureBox1_SizeModeChanged(object sender, EventArgs e)
        {
            if(sizeUp)
            {
                //pictureBox2.SizeMode = pictureBox1.SizeMode;
            }            
        }

        private void pictureBox1_BackgroundImageLayoutChanged(object sender, EventArgs e)
        {
            if(sizeUp)
            {
                //pictureBox2.BackgroundImageLayout = pictureBox1.BackgroundImageLayout;
            }            
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text        
                        
            Brush textBrush = new SolidBrush(textColor);
            //drawing.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            textColor = colorDialog1.Color;
            toolStripButton2.BackColor = textColor;
            getFrames();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            colorDialog2.ShowDialog();
            bgColor = colorDialog2.Color;
            toolStripButton3.BackColor = bgColor;
            getFrames();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //refresh with new size
            getFrames();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "AaBbCc123")
            {
                getFontFileLocation(myfont);
                Process.Start(@"C:\Windows\System32\fontview.exe", fontLocation);
            }        
        }

        private void getFontFileLocation(Font meFont)
        {            
            string fontName = meFont.Name;
            string[] fontDirectory = Directory.GetFiles(@"C:\WINDOWS\Fonts");
            string fileLoc = $@"C:\Windows\Fonts\{fontName}";

            if(File.Exists($"{fileLoc}.ttf"))
            {
                fontLocation = $"{fileLoc}.ttf";
            }
            else if(File.Exists($"{fileLoc}.otf"))
            {
                fontLocation = $"{fileLoc}.otf";
            }
            //else if (File.Exists($"{fileLoc}.sfd"))
            //{
            //    fontLocation = $"{fileLoc}.sfd";
            //}
            //else if (File.Exists($"{fileLoc}.vlw"))
            //{
            //    fontLocation = $"{fileLoc}.vlw";
            //}
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))//textbox can only accept numeric characters and nothing else
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))//textbox can only accept numeric characters and nothing else
            {
                e.Handled = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {            
            if (!(pause))
            {
                timer3.Stop();
            }
            
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                if (!(pause))
                {
                    timer3.Start();
                }
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            using (GifCreator gifCreator = AnimatedGif.Create(saveFileDialog1.FileName, 33))
            {                
                foreach (Image img in AsciiFrames)
                {                  
                    gifCreator.AddFrame(img, (int)GIFQuality.Default);                    
                }
            }
            if (!(pause))
            {
                timer3.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex != -1)
            {
               saveFileDialog2.ShowDialog();
            }        
            else
            {
                MessageBox.Show("Select a frame from the frames list to export.");
            }
        }

        private void saveFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            AsciiFrames[listBox1.SelectedIndex].Save(saveFileDialog2.FileName, ImageFormat.Png);//save still image of selected frame in Ascii art form as png
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace multi_sprite
{
    public partial class Form1 : Form
    {
        string path;
        bool file_valid;
        Thread TGetImage;
        Image image;
        List<PictureBox> SpriteSheets = new List<PictureBox>();
        int freeX = 0, freeY = 0;
        int deletion_index = 0;
        bool key_down = false;
        bool sprite_sheet_selected = false;
        public Form1()
        {
            InitializeComponent();
        }

        bool GetFileName(out string filename, DragEventArgs e) 
        {
            filename = String.Empty;
            bool file_proper = false;

            if((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array file_data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if(file_data == null)
                    return false;
                if((file_data.Length > 0) && (file_data.GetValue(0) is String))
                {
                    filename = ((string[])file_data)[0];
                    string extention = Path.GetExtension(filename).ToLower();
                    if(extention == ".png")
                    {
                        file_proper = true;
                    }
                }
            }
            return file_proper;
        }
        protected void LoadImage()
        {
            image = new Bitmap(path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            file_valid = GetFileName(out filename, e);
            if (file_valid)
            {
                path = filename;
                TGetImage = new Thread(new ThreadStart(LoadImage));
                TGetImage.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (file_valid)
            {
                while (TGetImage.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                PictureBox tempPictureBox = new PictureBox();
                tempPictureBox.Image = image;
                tempPictureBox.Location = new Point(freeX, freeY);
                tempPictureBox.Width = image.Width;
                tempPictureBox.Height = image.Height;
                tempPictureBox.Name = SpriteSheets.Count.ToString();
                tempPictureBox.Click += spriteSheet_OnClick;
                freeX += image.Width;

                SpriteSheets.Add(tempPictureBox);

                this.Controls.Add(tempPictureBox);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (key_down)
                return;
            if (!sprite_sheet_selected)
                return;
            if (SpriteSheets.Count < 1)
                return;
            if(e.KeyCode == Keys.Delete)
            {
                sprite_sheet_selected = false;
                key_down = true;
                SpriteSheets.RemoveAt(deletion_index);
                this.Controls.RemoveByKey(deletion_index.ToString());
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                key_down = false;
            }
        }

        private void btn_export_Click(object sender, EventArgs e)
        {
            if (SpriteSheets.Count < 1)
                return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = @"E:\";      
            saveFileDialog1.Title = "Save Sprite Sheet";
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "png";
            saveFileDialog1.Filter = "Image (*.png)|*.png";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream myStream;
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    Image exportImage = new Bitmap(SpriteSheets[0].Image);
                    for(int i = 1; i < SpriteSheets.Count; i++)
                    {
                        exportImage = MergedBitmaps(exportImage, SpriteSheets[i].Image as Bitmap, SpriteSheets[i].Location);
                    }
                    
                    exportImage.Save(myStream, System.Drawing.Imaging.ImageFormat.Png);
                    myStream.Close();
                }
            }
        }
        private Image MergedBitmaps(Image bmp1, Image bmp2, Point bmp2_location)
        {
            Image result = new Bitmap(bmp1.Width + bmp2.Width, bmp1.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp1, new Point(0,0));
                g.DrawImage(bmp2, bmp2_location);
               
            }
            return result;
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            SpriteSheets[deletion_index].BackColor = Color.Gray;
            deletion_index = 0;
            sprite_sheet_selected = false;
        }

        private void spriteSheet_OnClick(object sender, EventArgs e)
        {
            PictureBox tempPictureBox = sender as PictureBox;
            if (tempPictureBox == null)
                return;
            int index = Int32.Parse(tempPictureBox.Name);
            if(sprite_sheet_selected)
                SpriteSheets[deletion_index].BackColor = Color.Gray;

            deletion_index = index;
            SpriteSheets[index].BackColor = Color.Aquamarine;
            sprite_sheet_selected = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReadingMNISTDatabase
{
    public partial class Form1 : Form
    {
        //MNIST Data set
        MNIST_Database _MnistTrainingDatabase;
        MNIST_Database _MinstTestingDatabase;
        NumberInfo numbers;
        public Form1()
        {
            InitializeComponent();
            _MnistTrainingDatabase = new MNIST_Database();
            _MinstTestingDatabase = new MNIST_Database();
            numbers = new NumberInfo();
        }

        private void Read_Click(object sender, EventArgs e)
        {
            _MnistTrainingDatabase.LoadMinstFiles();
            _MinstTestingDatabase.LoadMinstFiles();
        }
        private void calculatemean()
        {
            ImagePattern img = null;
            int type = 0;
            int numberoftrained = Convert.ToInt32(textBox2.Text);
            if (numberoftrained > 60000)
                numberoftrained = 60000;
            for (int N = 0; N < numberoftrained; N++)//to loop train data
            {
                img = _MnistTrainingDatabase.m_pImagePatterns[(int)(N)];
                type = Convert.ToInt32(img.nLabel);
                numbers.numoftypes[type]++;
                int pixelvalue;
                for (int i = 0; i < 28*28; i++)
                {
                    if (img.pPattern[i] == 255)
                        pixelvalue = 0;
                    else
                        pixelvalue = 1;
                        numbers.mean[type, i] += pixelvalue;
                }
            }
            for (int k = 0; k < 10; k++)
            {
                for (int i = 0; i < 28*28; i++)
                {
                        numbers.mean[k, i] /= numbers.numoftypes[k];
                }
            }
        }

        private void calculatevar()
        {
            ImagePattern img = null;
            int type = 0;
            int numberoftrained = Convert.ToInt32(textBox2.Text);
            if (numberoftrained > 60000)
                numberoftrained = 60000;
            for (int i = 0; i < numberoftrained; i++)
            {
                img = _MnistTrainingDatabase.m_pImagePatterns[i];
                type = Convert.ToInt32(img.nLabel);
                int pixelvalue;
                for (int j = 0; j < img.pPattern.Count(); j++)
                {
                    if (img.pPattern[j] == 255)
                        pixelvalue = 0;
                    else
                        pixelvalue = 1;
                    numbers.varience[type, j] += Math.Pow((pixelvalue - numbers.mean[type, j]), 2);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < (28 * 28); j++)
                    numbers.varience[i, j] /= numbers.numoftypes[i];
            }
        }

        double normalfun(double x, double mu, double sgma)
        {
            double tmp = (1 / (Math.Sqrt(2 * Math.PI) * sgma)) * Math.Exp(-1 * Math.Pow((x - mu), 2) / (2 * (Math.Pow(sgma, 2))));
            return tmp;
        }
        double[,] classification = new double[10, 10];
        private int classify(ImagePattern img)
        {
            double[] cost = new double[10];
            for (int C = 0; C < 10; C++)
            {
                for (int i = 0; i < (28 * 28); i++)
                {
                    double mean = numbers.mean[C, i];
                    double varian = numbers.varience[C, i];
                    int pixelvalue;
                    if (img.pPattern[i] == 255)
                        pixelvalue = 0;
                    else
                        pixelvalue = 1;
                    if (varian == 0)
                        continue;
                    double tmp = normalfun(pixelvalue, mean, Math.Sqrt(varian));

                    if (cost[C] == 0 && tmp > 0)
                    {
                        cost[C] = tmp;
                        continue;
                    }
                    if (tmp > 0)
                        cost[C] = cost[C] * tmp;
                }
            }

            double mx = double.MinValue;
            int Mxcls = 0;
            for (int i = 0; i < 10; i++)
            {
                if (cost[i] > mx)
                {
                    mx = cost[i];
                    Mxcls = i;
                }
            }
            return Mxcls;
        }

        private void btn_Show_Click(object sender, EventArgs e)
        {
            Bitmap Bmap = new Bitmap(28, 28);
            int End = 28;
            int i = 0, j = 0, k = 0;
            byte PixelValue;
            ImagePattern IP = null;
            if (rdoBtn_TrainingSet.Checked)
                IP = _MnistTrainingDatabase.m_pImagePatterns[(int)(numericUpDown1.Value - 1)];
            else if (rdoBtn_TestingSet.Checked)
                IP = _MinstTestingDatabase.m_pImagePatterns[(int)(numericUpDown1.Value - 1)];
            while (i < 28)
            {
                k = 0;
                for (j = i * 28; j < End; j++)
                {
                    PixelValue = IP.pPattern[j];
                    if (chckBx_Threshold.Checked && PixelValue < 255)
                        PixelValue = 0;
                    Bmap.SetPixel(k, i, Color.FromArgb(PixelValue, PixelValue, PixelValue));
                    label1.Text = IP.nLabel.ToString();
                    k++;
                }
                i++;
                End = (i + 1) * 28;
            }
            pictureBox1.Image = (Image)Bmap;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = "Class" + i.ToString();
            }
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int[] testing = new int[10];

            ImagePattern img = null;
            classification = new double[10, 10];
            int numberofsamples = Convert.ToInt32(textBox1.Text);

            for (int N = 0; N < numberofsamples;N++ )
            {
                img = _MinstTestingDatabase.m_pImagePatterns[N];
                int type = Convert.ToInt32(img.nLabel);
                testing[type]++;
            }
            dataGridView2.Rows.Add();
            for (int i = 0; i < 10; i++)
                dataGridView2.Rows[0].Cells[i].Value = testing[i].ToString();

                for (int N = 0; N < numberofsamples; N++)
                {
                    double[] cost = new double[10];
                    img = _MinstTestingDatabase.m_pImagePatterns[N];
                    int type = Convert.ToInt32(img.nLabel);
                    int c = classify(img);
                    classification[type, c]++;
                }
            double accuracy = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = classification[i, j - 1];
                }
                accuracy += classification[i, i];
            }
            label2.Text = ((accuracy / numberofsamples) * 100).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImagePattern IP = null;
            if (rdoBtn_TestingSet.Checked)
            {
                IP = _MinstTestingDatabase.m_pImagePatterns[(int)(numericUpDown1.Value - 1)];
                label3.Text = classify(IP).ToString();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            calculatemean();
            calculatevar();
            MessageBox.Show("Training Is Over =D");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

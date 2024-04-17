using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading; // sleep

namespace Ayzenk2
{
    public partial class Form1 : Form
    {
        // ========= vxProgressBar vars
        private PictureBox[] stepSeparatorArray;
        // bool answered, int answer type, image
        // answer type: 0=false answer,1=true answer,2=skipped
        private LinkedList<ProgressBarElement> stepBoxList;
        //private List<int> skippedFrames;
        private int progressBarActiveElementIndex { get; set; }
        private bool MainSequenceCompleted { get; set; }
        public int MaxValue { get; set; }

        // ========= Main Frame vars
        private static LinkedList<(int, PictureBox)> listPics, listAnsw;

        private static int trueAnswer = 0;

        //Function to get random number
        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max) // включительно
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max + 1);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MaxValue = 10;
            MainSequenceCompleted = false;
            //skippedFrames = new List<int>();

            listPics = new LinkedList<(int, PictureBox)>(); // список картинок задачи

            listPics.AddLast((0, pictureBox1));
            listPics.AddLast((0, pictureBox2));
            listPics.AddLast((0, pictureBox3));
            listPics.AddLast((0, pictureBox4));
            listPics.AddLast((0, pictureBox5));
            listPics.AddLast((0, pictureBox6));
            listPics.AddLast((0, pictureBox7));
            listPics.AddLast((0, pictureBox8));
            listPics.AddLast((0, pictureBox9));
            
            int dbg1 = listPics.Count;

            listAnsw = new LinkedList<(int, PictureBox)>(); // список картинок ответов (10-15)

            listAnsw.AddLast((0, pictureBox10));
            listAnsw.AddLast((0, pictureBox11));
            listAnsw.AddLast((0, pictureBox12));
            listAnsw.AddLast((0, pictureBox13));
            listAnsw.AddLast((0, pictureBox14));
            listAnsw.AddLast((0, pictureBox15));
            
            int dbg2 = listAnsw.Count;

            groupBoxQuestion.Visible = false;
            groupBoxAnswer.Visible = false;
            buttonNext.Visible = false;
            buttonSkip.Visible = false;

            labelDbgLog.Text = string.Empty;

            // vxProgressBar init
            VxProgressBarInit();

            if (indexIsNotOwerflow())// ненужная проверка
            {
                LoadLearningEnvironment(progressBarActiveElementIndex + 1);
            }
        }

        private void LoadLearningEnvironment(int step)
        {
            setColor(Color.Black);

            var i = 1; // номер картинки (сквозная нумерация для задачи и ответов) 1 to 15s

            for (LinkedListNode<(int, PictureBox)> rn = listPics.First; rn != null; rn = rn.Next)
            {
                var Val = rn.Value;
                String fileName = $"C:/Users/verel/Desktop/MyAzencProg/ayzenc_details/{step}/a{step}_{i}.bmp";
                Val.Item2.Image = System.Drawing.Bitmap.FromFile(fileName);
                Val.Item2.SizeMode = PictureBoxSizeMode.StretchImage;
                rn.Value = Val;
                i++;
            }

            int dbg1 = listPics.Count;

            for (LinkedListNode<(int, PictureBox)> rn = listAnsw.First; rn != null; rn = rn.Next)
            {
                var Val = rn.Value;
                String fileName;
                if (i == 15)
                {
                    fileName = "C:/Users/verel/Desktop/MyAzencProg/ayzenc_details/__Q1.bmp";
                    Val.Item1 = 1; // помечаем правильный ответ
                }
                else
                {
                    fileName = $"C:/Users/verel/Desktop/MyAzencProg/ayzenc_details/{step}/a{step}_{i}.bmp";
                }
                Val.Item2.Image = System.Drawing.Bitmap.FromFile(fileName);
                Val.Item2.SizeMode = PictureBoxSizeMode.StretchImage;
                rn.Value = Val;
                i++;
            }

            int rnd1 = GetRandomNumber(0, 8); // от 0 до 8 включительно т.к. 9 картинок в вопросе

            // меняем местами рандомную картинку и картинку со знаком вопроса
            // listPics[rnd1].img - listAnsw[5].img
            //SwapAtoB(rnd1, 5, ref listPics, ref listAnsw); // 5 - т.к. в последнем элементе знак вопроса
            ExchangeQuestionImage(rnd1, ref listPics, ref listAnsw);

            // меняем местами картинки listAnsw[rnd2].img - listAnsw[5].img (только в ответах)
            // чтобы не было всегда одинаково
            int rnd2 = GetRandomNumber(0, 4); // от 0 до 4 включительно т.к. всего 6 картинок в ответах, а
            // 5 позицию не трогаем, ее мы будем менять...
            //SwapAtoB(rnd2, 5, ref listAnsw, ref listAnsw);
            MixAnswerImages(rnd2, ref listAnsw);
            int oldPosition = rnd2;

            // еще раз перемешаем ответы
            int rnd3 = GetRandomNumber(0, 5);
            if (rnd3 != oldPosition)
            {
                SwapAtoB(oldPosition, rnd3, ref listAnsw, ref listAnsw);
                // все равно плохо перемешано
            }

            trueAnswer = rnd3; // правильный ответ (нумерация с нуля)

            // TODO: перемешать весь список
            // in cycle descending
            // listAnswRand.AddLast((0, pictureBox10));

            Refresh();
        }

        private void VxProgressBarInit()
        {
            progressBarActiveElementIndex = 0;
            stepBoxList = new LinkedList<ProgressBarElement>();
            stepSeparatorArray = new PictureBox[MaxValue];

            for (int i = 0; i < MaxValue; i++)
            {
                var pbe = new PictureBox();
                pbe.Location = new Point(i * 65, 0);
                pbe.Width = 60;
                pbe.Height = 60;
                pbe.Tag = i; // uses in PrgrsbrElmt_Paint()
                pbe.BackColor = Color.MidnightBlue;
                pbe.Click += new System.EventHandler(PrgrsbrElmt_Click);
                pbe.Paint += new System.Windows.Forms.PaintEventHandler(PrgrsbrElmt_Paint);
                panel1.Controls.Add(pbe);

                var probarel = new ProgressBarElement();
                probarel.answered = false;
                probarel.answerType = 0;
                probarel.index = i;
                probarel.image = pbe;
                stepBoxList.AddLast(probarel);
                
                stepSeparatorArray[i] = new PictureBox();
                stepSeparatorArray[i].Location = new Point(i * 65 + 1, 0);
                stepSeparatorArray[i].Width = 10;
                stepSeparatorArray[i].Height = 60;
                stepSeparatorArray[i].Tag = i;
                stepSeparatorArray[i].BackColor = Color.White;
                panel1.Controls.Add(stepSeparatorArray[i]);
            }
        }

        void PrgrsbrElmt_Click(object sender, EventArgs e)
        {
            var control = sender as PictureBox;
            if (control != null)
            {
                switch ((e as MouseEventArgs).Button)
                {
                    case MouseButtons.Left:
                        if(MainSequenceCompleted)
                        {
                            //ShowSpecificFrame(Convert.ToInt32(control.Tag));
                            control.BackColor = Color.Black;
                        }
                        break;
                    case MouseButtons.Right:
                        control.BackColor = Color.Red;
                        break;
                    case MouseButtons.Middle:
                        control.BackColor = Color.SandyBrown;
                        break;
                }
            }
        }

        void PrgrsbrElmt_Paint(object sender, PaintEventArgs e)
        {
            var control = sender as PictureBox;
            if (control != null)
            {
                using (Font myFont = new Font("Arial", 8))
                {
                    var i = Int32.Parse(control.Tag.ToString());
                    i++;
                    e.Graphics.DrawString(i.ToString(), myFont, Brushes.White, new Point(2, 2));
                }
                ControlPaint.DrawBorder(e.Graphics, control.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
            }
        }

        public void setColor(Color color)
        {
            if (indexIsNotOwerflow())
            {
                var sbl = stepBoxList.ElementAt(progressBarActiveElementIndex);
                sbl.image.BackColor = color;
            }
        }

        public void PerformStep(Color color)
        {
            setColor(color);
            progressBarActiveElementIndex += 1;
        }

        // меняет местами картинки ListA[a].img - ListB[b].img
        private void SwapAtoB(int a, int b, ref LinkedList<(int, PictureBox)> La, ref LinkedList<(int, PictureBox)> Lb)
        {
            Image img = La.ElementAt(a).Item2.Image;

            La.ElementAt(a).Item2.Image = Lb.ElementAt(b).Item2.Image;
            Lb.ElementAt(b).Item2.Image = img;
        }

        private void ExchangeQuestionImage(int a, ref LinkedList<(int, PictureBox)> La, ref LinkedList<(int, PictureBox)> Lb)
        {
            SwapAtoB(a, 5, ref La, ref Lb);
        }

        private void MixAnswerImages(int a, ref LinkedList<(int, PictureBox)> La)
        {
            SwapAtoB(a, 5, ref La, ref La);
        }

        // Lp(Rnd1) <--> La(5) меняем местами рандомную фигуру в вопросах и знак вопроса из ответов
        // эта фигура будет правильным ответом (на позиции 5 от нуля)
        // La(Rnd2) <--> La(5) меняем местами рандомную фигуру из ответов и фигуру на позиции 5 из ответов
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Visible = false;

            // для того, чтобы впоследствии красиво проверить свойство checked
            groupBoxAnswer.Controls.Clear();
            groupBoxAnswer.Controls.Add(radioButton1); // кнопки выбора
            groupBoxAnswer.Controls.Add(radioButton2);
            groupBoxAnswer.Controls.Add(radioButton3);
            groupBoxAnswer.Controls.Add(radioButton4);
            groupBoxAnswer.Controls.Add(radioButton5);
            groupBoxAnswer.Controls.Add(radioButton6);
            //
            groupBoxAnswer.Controls.Add(pictureBox10); // картинки
            groupBoxAnswer.Controls.Add(pictureBox11);
            groupBoxAnswer.Controls.Add(pictureBox12);
            groupBoxAnswer.Controls.Add(pictureBox13);
            groupBoxAnswer.Controls.Add(pictureBox14);
            groupBoxAnswer.Controls.Add(pictureBox15);
            //
            groupBoxQuestion.Visible = true;
            groupBoxAnswer.Visible = true;
            buttonStart.Visible = false;
            buttonNext.Visible = true;
            buttonSkip.Visible = true;
            //
            labelDbgLog.Text = " ";// $"Rnd = {rnd1}, Rnd3 = {rnd3}";
        }




        private void buttonNext_Click(object sender, EventArgs e) // Кнопка "Ответить"
        {
            try
            {
                var checkedButton = groupBoxAnswer.Controls.OfType<RadioButton>()
                                      .FirstOrDefault(r => r.Checked);

                if (checkedButton != null)
                {
                    var sbl = stepBoxList.ElementAt(progressBarActiveElementIndex);
                    int oneBazedAnswer = trueAnswer + 1;
                    if (checkedButton.Text.Equals(oneBazedAnswer.ToString())) // верный ответ
                    {
                        if (sbl != null)
                        {
                            sbl.answered = true;
                            sbl.answerType = 1;
                        }
                        PerformStep(Color.Green); // делает Value++
                    }
                    else // неправильный ответ
                    {
                        if (sbl != null)
                        {
                            sbl.answered = true;
                            sbl.answerType = 0;
                        }
                        PerformStep(Color.Red); // делает Value++
                    }
                    //
                    if (indexIsNotOwerflow() && !MainSequenceCompleted)
                    {
                        LoadLearningEnvironment(progressBarActiveElementIndex + 1);
                    }
                    else
                    {
                        MainSequenceCompleted = true;
                        if (GetSkippedElements())
                        {
                            LoadLearningEnvSkipped();
                        }
                        else
                        {
                            ShowResult();
                        }
                    }
                }
                else { MessageBox.Show("Вариант ответа не выбран!"); }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool indexIsNotOwerflow()
        {
            if (progressBarActiveElementIndex < MaxValue)
            {
                return true;
            }
            return false;
        }

        private void LoadLearningEnvSkipped()
        {
            foreach (ProgressBarElement pbe in stepBoxList)
            {
                if (pbe.answerType == 2) // 2=skipped answer
                {
                    progressBarActiveElementIndex = pbe.index;
                    LoadLearningEnvironment(pbe.index + 1);
                    return;
                }
            }
        }

        private bool GetSkippedElements()
        {
            foreach(ProgressBarElement pbe in stepBoxList)
            {
                if (pbe.answerType == 2) // 2=skipped answer
                {
                    return true;
                }
            }
            return false;
        }

        private void ShowResult()
        {
            string str = $"согласно данного теста ваш IQ по 10-балльной системе = {GetTrueAnswersCount()}";
            MessageBox.Show(str,"All completed!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }

        private int GetTrueAnswersCount()
        {
            var i = 0;
            foreach (ProgressBarElement pbe in stepBoxList)
            {
                if (pbe.answerType == 1) // 1= true answer
                {
                    i++;
                }
            }
            return i;
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            if(progressBarActiveElementIndex - 1 >= MaxValue) // последний элемент
            { 
                return; 
            } 
            else 
            {
                PerformStep(Color.SandyBrown);
                buttonNext.Text = progressBarActiveElementIndex.ToString();
            }
            
            if (indexIsNotOwerflow())
            {
                LoadLearningEnvironment(progressBarActiveElementIndex + 1);
            }
            else
            {
                MainSequenceCompleted = true;
            }
        }
    }
}

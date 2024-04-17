using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayzenk2
{
    internal class ProgressBarElement
    {
        // bool answered, int answer type, image
        // answer type: 0=false answer,1=true answer,2=skipped
        public bool answered { get; set; }
        public int answerType { get; set; }
        public int index { get; set; }
        public PictureBox image { get; set; }

        public ProgressBarElement()
        {
            this.answered = false;
            this.answerType = 2;
            this.index = 0;
            this.image = null;
        }
        public ProgressBarElement(bool answ, int answerTyp, int idx, PictureBox imag)
        {
            this.answered = answ;
            this.answerType = answerTyp;
            this.index = idx;
            this.image = imag;
        }
    }
}

using System;
using System.Windows.Forms;
using ChessEngine.Main;

namespace ChessEngine
{
    public partial class form : Form
    {
        public form()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
            
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Enter)
            {
                Board board= new Board();
                board.Fen = txtCommand.Text;
                txtCommand.Text = "";
                txtOut.Text = "";
                for (int i = 1; i < 7; i++)
                {
                    txtOut.Text += Test.MoveGeneration.MinMax(board, i) + Environment.NewLine;
                    txtOut.Refresh();
                    Refresh();

                    
                }
                
            }
        }
    }
}

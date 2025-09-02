//using System.Diagnostics;
//using System.Text;

namespace Cave
{
    /// <summary>
    /// This runs first.
    /// </summary>
    /// <remarks>
    /// https://www.roguebasin.com/index.php/Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels
    /// </remarks>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            /*
            var width = 100; //has to be >=5.
            var height = 20;

            var walls = new CellularAutomata(width, height).Generate(5); //4 iters, 40% wall

            for (int y = 0; y < height; y++)
            {
                var st = new StringBuilder();
                for (int x = 0; x < width; x++)
                {
                    st.Append(walls[x , y] ? '#' : ' ');
                }
                Debug.WriteLine(st.ToString());
            }*/
        }
    }
}

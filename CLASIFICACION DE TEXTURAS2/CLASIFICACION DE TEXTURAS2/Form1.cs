using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CLASIFICACION_DE_TEXTURAS1
{
    public partial class Form1 : Form
    {
        Bitmap imagenOriginal;
        Bitmap imagenSuavizada;

        SqlConnection conexion = new SqlConnection(
        @"Server=.;
        Database=dbtexturas;
        Trusted_Connection=True;");

        int tamBloque = 3;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();

            abrir.Filter = "Imagenes|*.jpg;*.png;*.bmp";

            if (abrir.ShowDialog() == DialogResult.OK)
            {
                imagenOriginal = new Bitmap(abrir.FileName);

                pictureBox1.Image = imagenOriginal;

                pictureBox2.Image = null;
            }
        }

        private Color ObtenerPromedioBloque(Bitmap bmp, int x, int y)
        {
            int sumaR = 0;
            int sumaG = 0;
            int sumaB = 0;

            int contador = 0;

            for (int i = x; i < x + tamBloque; i++)
            {
                for (int j = y; j < y + tamBloque; j++)
                {
                    if (i < bmp.Width && j < bmp.Height)
                    {
                        Color c = bmp.GetPixel(i, j);

                        sumaR += c.R;
                        sumaG += c.G;
                        sumaB += c.B;

                        contador++;
                    }
                }
            }

            int r = sumaR / contador;
            int g = sumaG / contador;
            int b = sumaB / contador;

            return Color.FromArgb(r, g, b);
        }


        Color colorBloque;
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //if (imagenOriginal == null)
            //    return;

           // Bitmap bmp = new Bitmap(imagenOriginal);

            //int x = e.X;
           // int y = e.Y;

           // if (x >= bmp.Width || y >= bmp.Height)
             //   return;

            //colorBloque = ObtenerPromedioBloque(bmp, x, y);

            //label1.Text =
              //  "RGB Promedio: " +
                //colorBloque.R + "," +
               // colorBloque.G + "," +
                //colorBloque.B;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string clase = comboBox1.Text;

            conexion.Open();

            SqlCommand cmd = new SqlCommand(
            @"INSERT INTO Muestras2
            (Clase,RPromedio,GPromedio,BPromedio)
            VALUES
            (@Clase,@R,@G,@B)", conexion);

            cmd.Parameters.AddWithValue("@Clase", clase);

            cmd.Parameters.AddWithValue("@R", colorBloque.R);

            cmd.Parameters.AddWithValue("@G", colorBloque.G);

            cmd.Parameters.AddWithValue("@B", colorBloque.B);

            cmd.ExecuteNonQuery();

            conexion.Close();

            MessageBox.Show("Muestra guardada");
        }

        private double Distancia(Color c1, Color c2)
        {
            int r = c1.R - c2.R;
            int g = c1.G - c2.G;
            int b = c1.B - c2.B;

            return Math.Sqrt(r * r + g * g + b * b);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap resultado = new Bitmap(imagenSuavizada);

            List<Muestra> muestras = ObtenerMuestras();

            for (int y = 0; y < resultado.Height; y += tamBloque)
            {
                for (int x = 0; x < resultado.Width; x += tamBloque)
                {
                    Color bloque =
                        ObtenerPromedioBloque(resultado, x, y);

                    string clase =
                        ClasificarBloque(bloque, muestras);

                    Color colorFinal = Color.White;

                    if (clase == "Otro")
                        continue;

                    else if (clase == "Asfalto")
                        colorFinal = Color.Black;

                    else if (clase == "Tierra")
                        colorFinal = Color.Brown;

                    else if (clase == "Cemento")
                        colorFinal = Color.Gray;
                    else if (clase == "Agua")
                        colorFinal = Color.Blue;
                    else if (clase == "Cesped")
                        colorFinal = Color.Green;
               

                    for (int i = x; i < x + tamBloque; i++)
                    {
                        for (int j = y; j < y + tamBloque; j++)
                        {
                            if (i < resultado.Width &&
                                j < resultado.Height)
                            {
                                resultado.SetPixel(i, j, colorFinal);
                            }
                        }
                    }
                }
            }

            pictureBox2.Image = resultado;
        }

        public class Muestra
        {
            public string Clase { get; set; }

            public Color ColorPromedio { get; set; }
        }

        private List<Muestra> ObtenerMuestras()
        {
            List<Muestra> lista =
                new List<Muestra>();

            conexion.Open();

            SqlCommand cmd =
                new SqlCommand("SELECT * FROM Muestras",
                conexion);

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Muestra m = new Muestra();

                m.Clase = dr["Clase"].ToString();

                int r =
                    Convert.ToInt32(dr["RPromedio"]);

                int g =
                    Convert.ToInt32(dr["GPromedio"]);

                int b =
                    Convert.ToInt32(dr["BPromedio"]);

                m.ColorPromedio =
                    Color.FromArgb(r, g, b);

                lista.Add(m);
            }

            conexion.Close();

            return lista;
        }

        private string ClasificarBloque(Color bloque,List<Muestra> muestras)
        {
            double menor = double.MaxValue;

            string claseFinal = "";

            foreach (Muestra m in muestras)
            {
                double d =
                    Distancia(bloque,
                    m.ColorPromedio);

                if (d < menor)
                {
                    menor = d;

                    claseFinal = m.Clase;
                }
            }

            return claseFinal;
        }

        //fase suavizado de imagen
        private Bitmap AplicarFiltroPromedio(Bitmap imagen)
        {
            Bitmap resultado =
                new Bitmap(imagen.Width, imagen.Height);

            for (int y = 1; y < imagen.Height - 1; y++)
            {
                for (int x = 1; x < imagen.Width - 1; x++)
                {
                    int sumaR = 0;
                    int sumaG = 0;
                    int sumaB = 0;

                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            Color c =
                                imagen.GetPixel(x + i, y + j);

                            sumaR += c.R;
                            sumaG += c.G;
                            sumaB += c.B;
                        }
                    }

                    resultado.SetPixel(
                        x,
                        y,
                        Color.FromArgb(
                            sumaR / 9,
                            sumaG / 9,
                            sumaB / 9));
                }
            }

            return resultado;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (imagenOriginal == null)
            {
                MessageBox.Show(
                    "Debe cargar una imagen.");
                return;
            }

            imagenSuavizada =
                AplicarFiltroPromedio(imagenOriginal);

            pictureBox2.Image =
                imagenSuavizada;

            MessageBox.Show(
                "Filtro aplicado correctamente.");
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (imagenSuavizada == null)
                return;

            Bitmap bmp = new Bitmap(imagenSuavizada);

            int x = e.X;
            int y = e.Y;

            if (x >= bmp.Width || y >= bmp.Height)
                return;

            colorBloque = ObtenerPromedioBloque(bmp, x, y);

            label1.Text =
                "RGB Promedio: " +
                colorBloque.R + "," +
                colorBloque.G + "," +
                colorBloque.B;

        }



    }
}

using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace EstructurasDeDatosIntegrador
{
    public partial class Form1 : Form
    {
        private const int CapacidadTotal = 50;

        private ElementHost  _videoHost;
        private MediaElement _mediaElement;

        public Form1()
        {
            InitializeComponent();
            lblCapTotal.Text = CapacidadTotal.ToString();
            ActualizarOcupacion(0);
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            InicializarVideo();
        }

        // Crea un MediaElement de WPF hospedado en un ElementHost de WinForms
        // y lo superpone exactamente sobre picCamara.
        private void InicializarVideo()
        {
            string videoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "loop.mp4");

            if (!File.Exists(videoPath)) return;

            _mediaElement = new MediaElement
            {
                Source             = new Uri(videoPath, UriKind.Absolute),
                LoadedBehavior     = MediaState.Manual,
                UnloadedBehavior   = MediaState.Stop,
                Stretch            = Stretch.Uniform,
                Volume             = 0
            };

            // Al terminar el video, reinicia desde el principio → loop manual.
            _mediaElement.MediaEnded += (s, e) =>
            {
                _mediaElement.Position = TimeSpan.Zero;
                _mediaElement.Play();
            };

            _videoHost = new ElementHost
            {
                Location = picCamara.Location,
                Size     = picCamara.Size,
                Child    = _mediaElement
            };

            grpCamara.Controls.Add(_videoHost);
            _videoHost.BringToFront();

            _mediaElement.Play();
        }

        // Actualiza los contadores de ocupación y disponibilidad.
        private void ActualizarOcupacion(int ocupados)
        {
            lblOcupados.Text    = ocupados.ToString();
            lblDisponibles.Text = (CapacidadTotal - ocupados).ToString();
        }

        // Reloj en tiempo real — dispara cada segundo.
        private void timerReloj_Tick(object sender, EventArgs e)
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // Evita que Enter inserte un salto de línea en el campo multiline.
        private void txtPlaca_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
                e.Handled = true;
        }

        // Dibuja "Sin señal" cuando no hay video ni imagen en picCamara.
        private void picCamara_Paint(object sender, PaintEventArgs e)
        {
            if (picCamara.Image != null) return;

            const string texto = "Sin señal de cámara";
            using var fuente = new Font("Segoe UI", 12F);
            using var pincel = new SolidBrush(Color.FromArgb(90, 90, 90));
            var tam = e.Graphics.MeasureString(texto, fuente);
            e.Graphics.DrawString(
                texto, fuente, pincel,
                (picCamara.Width  - tam.Width)  / 2f,
                (picCamara.Height - tam.Height) / 2f);
        }

        // Lógica de registro — se completará en pasos siguientes.
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            string placa = txtPlaca.Text.Trim();
            if (string.IsNullOrEmpty(placa))
            {
                MessageBox.Show("Ingrese la placa del vehículo.", "Campo requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPlaca.Focus();
                return;
            }

            // TODO: registrar entrada en el sistema de almacenamiento.
            MessageBox.Show($"Entrada registrada: {placa}", "Registro exitoso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtPlaca.Clear();
            txtPlaca.Focus();
        }
    }
}

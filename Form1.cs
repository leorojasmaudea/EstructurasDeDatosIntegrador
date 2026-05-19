using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Color = System.Drawing.Color;
using EstructurasDeDatosIntegrador.Storage;
using Control = System.Windows.Forms.Control;

namespace EstructurasDeDatosIntegrador
{
    public partial class Form1 : Form
    {
        private const int CapacidadTotal = 50;

        private ElementHost  _videoHost;
        private MediaElement _mediaElement;

        private readonly HashingStorageVehiculos _storage = new HashingStorageVehiculos();

        public Form1()
        {
            InitializeComponent();
            lblCapTotal.Text = CapacidadTotal.ToString();

            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VehicleData");
            Directory.CreateDirectory(dataDir);
            Directory.SetCurrentDirectory(dataDir);
            _storage.EnsureInitialized();

            ActualizarOcupacion(int.Parse(_storage.GetVehiculoCount()));
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
                Source           = new Uri(videoPath, UriKind.Absolute),
                LoadedBehavior   = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
                Stretch          = Stretch.Uniform,
                Volume           = 0,
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
                Child    = _mediaElement,
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

        // Busca la placa en tiempo real con cada tecla presionada.
        private void txtPlaca_TextChanged(object sender, EventArgs e)
        {
            string placa    = txtPlaca.Text.Trim();
            bool   presente = !string.IsNullOrEmpty(placa) && _storage.GetVehiculo(placa) != null;

            btnRegistrar.Text      = presente ? "REGISTRAR SALIDA"           : "REGISTRAR ENTRADA";
            btnRegistrar.BackColor = presente ? Color.FromArgb(160, 30, 30)  : Color.FromArgb(30, 47, 78);
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

            if (btnRegistrar.Text == "REGISTRAR SALIDA")
            {
                RegistrarSalida(placa);
                return;
            }

            RegistrarEntrada(placa);
        }

        private void btnVerTodos_Click(object sender, EventArgs e)
        {
            var ventana = new VentanaVehiculos(_storage.GetVehiculosPresentes());
            ventana.ShowDialog(this);
        }

        private void btnTarifas_Click(object sender, EventArgs e)
        {
            new VentanaTarifas().ShowDialog(this);
        }

        private void RegistrarEntrada(string placa)
        {
            // Capturar antes de abrir el diálogo para que no aparezca en la foto.
            byte[] foto = CapturarFotoVideo();

            using var dialog = new DialogIngreso(placa);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var vehiculo = new Vehiculo(placa, dialog.TipoSeleccionado, dialog.TarifaSeleccionada,
                                        DateTime.Now, dialog.Comentarios, foto);

            if (!_storage.AddVehiculo(vehiculo))
            {
                MessageBox.Show($"La placa {placa} ya está registrada.", "Duplicado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActualizarOcupacion(int.Parse(_storage.GetVehiculoCount()));
            txtPlaca.Clear();
            txtPlaca.Focus();
        }

        private void RegistrarSalida(string placa)
        {
            var vehiculo = _storage.GetVehiculo(placa);
            if (vehiculo == null) return;

            using var dialog = new DialogLiquidacion(vehiculo);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            _storage.DeleteVehiculo(placa);
            ActualizarOcupacion(int.Parse(_storage.GetVehiculoCount()));
            txtPlaca.Clear();
            txtPlaca.Focus();
        }

        // Captura la imagen de la zona de cámara usando las coordenadas de pantalla.
        private byte[] CapturarFotoVideo()
        {
            Control camCtrl = _videoHost as Control ?? picCamara;
            try
            {
                var screenPt = picCamara.PointToScreen(System.Drawing.Point.Empty);
                using var bmp = new Bitmap(camCtrl.Width, camCtrl.Height);
                using var g   = Graphics.FromImage(bmp);
                g.CopyFromScreen(screenPt, System.Drawing.Point.Empty, camCtrl.Size);
                using var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
            catch
            {
                return System.Array.Empty<byte>();
            }
        }
    }
}

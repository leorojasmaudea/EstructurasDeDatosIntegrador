using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EstructurasDeDatosIntegrador.Storage;

namespace EstructurasDeDatosIntegrador
{
    internal class VentanaVehiculos : Form
    {
        private readonly ListView   _lista;
        private readonly Label      _lblPlacaVal;
        private readonly Label      _lblTipoVal;
        private readonly Label      _lblTarifaVal;
        private readonly Label      _lblComentariosVal;
        private readonly PictureBox _picFoto;

        public VentanaVehiculos(List<Vehiculo> vehiculos)
        {
            Text            = "Vehículos en el parqueadero";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(860, 520);
            BackColor       = Color.FromArgb(240, 242, 245);

            // ── Panel izquierdo: lista ─────────────────────────────────────────
            var grpLista = new GroupBox
            {
                Text     = "Vehículos presentes",
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size     = new Size(290, 490),
            };

            _lista = new ListView
            {
                Location      = new Point(8, 22),
                Size          = new Size(274, 460),
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                HideSelection = false,
                Font          = new Font("Segoe UI", 10F),
            };
            _lista.Columns.Add("Placa",  100);
            _lista.Columns.Add("Tipo",   110);
            _lista.Columns.Add("Tarifa", 60);
            _lista.SelectedIndexChanged += OnSeleccionCambiada;
            grpLista.Controls.Add(_lista);

            // ── Panel derecho: detalle ─────────────────────────────────────────
            var grpDetalle = new GroupBox
            {
                Text     = "Detalle del vehículo",
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(310, 10),
                Size     = new Size(540, 490),
            };

            grpDetalle.Controls.Add(EtiquetaTitulo("Placa:",        30));
            _lblPlacaVal = EtiquetaValor(30);
            grpDetalle.Controls.Add(_lblPlacaVal);

            grpDetalle.Controls.Add(EtiquetaTitulo("Tipo:",         58));
            _lblTipoVal = EtiquetaValor(58);
            grpDetalle.Controls.Add(_lblTipoVal);

            grpDetalle.Controls.Add(EtiquetaTitulo("Tarifa:",       86));
            _lblTarifaVal = EtiquetaValor(86);
            grpDetalle.Controls.Add(_lblTarifaVal);

            grpDetalle.Controls.Add(EtiquetaTitulo("Comentarios:", 114));
            _lblComentariosVal = new Label
            {
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(130, 114),
                Size      = new Size(395, 40),
                AutoSize  = false,
            };
            grpDetalle.Controls.Add(_lblComentariosVal);

            grpDetalle.Controls.Add(new Label
            {
                Text     = "Foto de ingreso:",
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(15, 160),
                AutoSize = true,
            });

            _picFoto = new PictureBox
            {
                Location    = new Point(15, 180),
                Size        = new Size(510, 288),
                SizeMode    = PictureBoxSizeMode.Zoom,
                BackColor   = Color.FromArgb(18, 18, 18),
                BorderStyle = BorderStyle.FixedSingle,
            };
            grpDetalle.Controls.Add(_picFoto);

            Controls.Add(grpLista);
            Controls.Add(grpDetalle);

            CargarLista(vehiculos);
        }

        private static Label EtiquetaTitulo(string texto, int y) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location  = new Point(15, y),
            Size      = new Size(115, 24),
            TextAlign = ContentAlignment.MiddleLeft,
        };

        private static Label EtiquetaValor(int y) => new Label
        {
            Font      = new Font("Segoe UI", 10F),
            Location  = new Point(130, y),
            Size      = new Size(395, 24),
            TextAlign = ContentAlignment.MiddleLeft,
        };

        private void CargarLista(List<Vehiculo> vehiculos)
        {
            _lista.Items.Clear();
            foreach (var v in vehiculos)
            {
                var item = new ListViewItem(v.Placa);
                item.SubItems.Add(v.Tipo.ToString());
                item.SubItems.Add(v.Tarifa == Storage.TipoTarifa.PorHora ? "Hora" : "Día");
                item.Tag = v;
                _lista.Items.Add(item);
            }
        }

        private void OnSeleccionCambiada(object sender, EventArgs e)
        {
            if (_lista.SelectedItems.Count == 0) { LimpiarDetalle(); return; }

            var v = (Vehiculo)_lista.SelectedItems[0].Tag;

            _lblPlacaVal.Text       = v.Placa;
            _lblTipoVal.Text        = v.Tipo.ToString();
            _lblTarifaVal.Text      = v.Tarifa == Storage.TipoTarifa.PorHora ? "Por hora" : "Por día";
            _lblComentariosVal.Text = string.IsNullOrEmpty(v.Comentarios) ? "—" : v.Comentarios;

            _picFoto.Image = (v.Foto != null && v.Foto.Length > 0)
                ? Image.FromStream(new MemoryStream(v.Foto))
                : null;
        }

        private void LimpiarDetalle()
        {
            _lblPlacaVal.Text       = string.Empty;
            _lblTipoVal.Text        = string.Empty;
            _lblTarifaVal.Text      = string.Empty;
            _lblComentariosVal.Text = string.Empty;
            _picFoto.Image          = null;
        }
    }
}

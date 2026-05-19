using System;
using System.Drawing;
using System.Windows.Forms;
using EstructurasDeDatosIntegrador.Storage;

namespace EstructurasDeDatosIntegrador
{
    internal class DialogIngreso : Form
    {
        private readonly ComboBox cmbTipo;
        private readonly TextBox  txtComentarios;

        public TipoVehiculo TipoSeleccionado => (TipoVehiculo)cmbTipo.SelectedIndex;
        public string       Comentarios      => txtComentarios.Text.Trim();

        public DialogIngreso(string placa)
        {
            Text            = $"Ingreso — {placa}";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(340, 230);
            BackColor       = Color.FromArgb(240, 242, 245);

            var lblTipo = new Label
            {
                Text      = "Tipo de vehículo:",
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(16, 16),
                Size      = new Size(308, 22),
            };

            cmbTipo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10F),
                Location      = new Point(16, 40),
                Size          = new Size(308, 26),
            };
            foreach (var val in Enum.GetValues(typeof(TipoVehiculo)))
                cmbTipo.Items.Add(val.ToString());
            cmbTipo.SelectedIndex = 0;

            var lblCom = new Label
            {
                Text     = "Comentarios (opcional):",
                Font     = new Font("Segoe UI", 10F),
                Location = new Point(16, 80),
                Size     = new Size(308, 22),
            };

            txtComentarios = new TextBox
            {
                Multiline  = true,
                Font       = new Font("Segoe UI", 10F),
                Location   = new Point(16, 104),
                Size       = new Size(308, 64),
                ScrollBars = ScrollBars.Vertical,
            };

            var btnContinuar = new Button
            {
                Text         = "Continuar",
                Font         = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor    = Color.FromArgb(30, 47, 78),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat,
                Location     = new Point(96, 180),
                Size         = new Size(148, 36),
                DialogResult = DialogResult.OK,
            };
            btnContinuar.FlatAppearance.BorderSize = 0;

            AcceptButton = btnContinuar;
            Controls.AddRange(new Control[] { lblTipo, cmbTipo, lblCom, txtComentarios, btnContinuar });
        }
    }
}

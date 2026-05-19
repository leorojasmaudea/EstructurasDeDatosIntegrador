using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EstructurasDeDatosIntegrador.Storage;

namespace EstructurasDeDatosIntegrador
{
    internal class VentanaTarifas : Form
    {
        private readonly HashingStorageTarifas _storage   = new HashingStorageTarifas();
        private readonly string                _tarifsDir;
        private readonly string                _returnDir;

        // Panel izquierdo
        private readonly ListView _lista;

        // Panel derecho — campos de edición
        private readonly TextBox  _txtCodigo;
        private readonly TextBox  _txtNombre;
        private readonly ComboBox _cmbTipo;
        private readonly ComboBox _cmbAplicaA;
        private readonly TextBox  _txtValor;
        private readonly ComboBox _cmbHoraInicio;
        private readonly ComboBox _cmbHoraFin;
        private readonly TextBox  _txtDescripcion;
        private readonly Button   _btnCrear;
        private readonly Button   _btnEliminar;
        private readonly Button   _btnLimpiar;

        public VentanaTarifas()
        {
            _returnDir = Directory.GetCurrentDirectory();
            _tarifsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TarifsData");
            Directory.CreateDirectory(_tarifsDir);
            Directory.SetCurrentDirectory(_tarifsDir);
            _storage.EnsureInitialized();

            // ── Propiedades del formulario ─────────────────────────────────────
            Text            = "Administración de Tarifas";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(960, 600);
            BackColor       = Color.FromArgb(240, 242, 245);

            // ── Panel izquierdo: lista ─────────────────────────────────────────
            var grpLista = new GroupBox
            {
                Text     = "Tarifas registradas",
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size     = new Size(355, 572),
            };

            _lista = new ListView
            {
                Location      = new Point(8, 22),
                Size          = new Size(339, 542),
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = true,
                HideSelection = false,
                Font          = new Font("Segoe UI", 9.5F),
            };
            _lista.Columns.Add("Código",   64);
            _lista.Columns.Add("Nombre",   88);
            _lista.Columns.Add("Tipo",     58);
            _lista.Columns.Add("Aplica a", 65);
            _lista.Columns.Add("Horario",  70);
            _lista.Columns.Add("Valor",    52);
            _lista.SelectedIndexChanged += OnSeleccionCambiada;
            grpLista.Controls.Add(_lista);

            // ── Panel derecho: detalle / nueva tarifa ──────────────────────────
            var grpDetalle = new GroupBox
            {
                Text     = "Nueva tarifa",
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(375, 10),
                Size     = new Size(572, 572),
            };

            // Código
            grpDetalle.Controls.Add(Etiqueta("Código:", 30));
            _txtCodigo = Campo(30, 195);
            _txtCodigo.MaxLength = 8;
            grpDetalle.Controls.Add(_txtCodigo);
            grpDetalle.Controls.Add(new Label
            {
                Text      = "(máx. 8 caracteres)",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = SystemColors.GrayText,
                Location  = new Point(340, 33),
                AutoSize  = true,
            });

            // Nombre
            grpDetalle.Controls.Add(Etiqueta("Nombre:", 67));
            _txtNombre = Campo(67, 415);
            grpDetalle.Controls.Add(_txtNombre);

            // Tipo
            grpDetalle.Controls.Add(Etiqueta("Tipo:", 104));
            _cmbTipo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10F),
                Location      = new Point(130, 101),
                Size          = new Size(165, 26),
            };
            _cmbTipo.Items.AddRange(new object[] { "Por hora", "Por día" });
            _cmbTipo.SelectedIndex = 0;
            grpDetalle.Controls.Add(_cmbTipo);

            // Aplica a
            grpDetalle.Controls.Add(Etiqueta("Aplica a:", 141));
            _cmbAplicaA = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10F),
                Location      = new Point(130, 138),
                Size          = new Size(200, 26),
            };
            _cmbAplicaA.Items.Add("Todos los tipos");
            foreach (var v in Enum.GetValues(typeof(TipoVehiculo)))
                _cmbAplicaA.Items.Add(v.ToString());
            _cmbAplicaA.SelectedIndex = 0;
            grpDetalle.Controls.Add(_cmbAplicaA);

            // Valor
            grpDetalle.Controls.Add(Etiqueta("Valor ($):", 178));
            _txtValor = Campo(178, 120);
            grpDetalle.Controls.Add(_txtValor);

            // Horario
            grpDetalle.Controls.Add(Etiqueta("Horario:", 215));
            grpDetalle.Controls.Add(new Label
            {
                Text      = "De",
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(130, 218),
                Size      = new Size(22, 22),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            _cmbHoraInicio = HoraCombo(155, 0, 23);
            grpDetalle.Controls.Add(_cmbHoraInicio);
            grpDetalle.Controls.Add(new Label
            {
                Text      = "a",
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(243, 218),
                Size      = new Size(16, 22),
                TextAlign = ContentAlignment.MiddleCenter,
            });
            _cmbHoraFin = HoraCombo(262, 1, 24);
            grpDetalle.Controls.Add(_cmbHoraFin);
            grpDetalle.Controls.Add(new Label
            {
                Text      = "h",
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(350, 218),
                Size      = new Size(16, 22),
                TextAlign = ContentAlignment.MiddleLeft,
            });

            // Descripción
            grpDetalle.Controls.Add(new Label
            {
                Text     = "Descripción:",
                Font     = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, 254),
                AutoSize = true,
            });
            _txtDescripcion = new TextBox
            {
                Multiline  = true,
                Font       = new Font("Segoe UI", 10F),
                Location   = new Point(10, 274),
                Size       = new Size(545, 70),
                ScrollBars = ScrollBars.Vertical,
            };
            grpDetalle.Controls.Add(_txtDescripcion);

            // ── Botones de acción ──────────────────────────────────────────────
            _btnCrear = new Button
            {
                Text          = "CREAR TARIFA",
                Font          = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor     = Color.FromArgb(30, 47, 78),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
                Location      = new Point(10, 362),
                Size          = new Size(545, 42),
                Cursor        = Cursors.Hand,
            };
            _btnCrear.FlatAppearance.BorderSize = 0;
            _btnCrear.Click += BtnCrear_Click;
            grpDetalle.Controls.Add(_btnCrear);

            _btnEliminar = new Button
            {
                Text          = "ELIMINAR TARIFA SELECCIONADA",
                Font          = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor     = Color.FromArgb(160, 30, 30),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
                Location      = new Point(10, 414),
                Size          = new Size(545, 42),
                Cursor        = Cursors.Hand,
                Enabled       = false,
            };
            _btnEliminar.FlatAppearance.BorderSize = 0;
            _btnEliminar.Click += BtnEliminar_Click;
            grpDetalle.Controls.Add(_btnEliminar);

            _btnLimpiar = new Button
            {
                Text          = "Limpiar formulario",
                Font          = new Font("Segoe UI", 9.5F),
                BackColor     = Color.White,
                ForeColor     = Color.FromArgb(80, 80, 80),
                FlatStyle     = FlatStyle.Flat,
                Location      = new Point(10, 466),
                Size          = new Size(545, 32),
                Cursor        = Cursors.Hand,
            };
            _btnLimpiar.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            _btnLimpiar.FlatAppearance.BorderSize  = 1;
            _btnLimpiar.Click += (s, e) => LimpiarFormulario();
            grpDetalle.Controls.Add(_btnLimpiar);

            // Valores por defecto del horario
            _cmbHoraInicio.SelectedIndex = 8;  // 8:00
            _cmbHoraFin.SelectedIndex    = 9;  // 18:00 (índice 17 sería 18, pero ajustamos: índice=valor-1 → índice 17 = 18:00)

            Controls.Add(grpLista);
            Controls.Add(grpDetalle);

            CargarLista();
        }

        // ── Helpers para construir controles ──────────────────────────────────

        private static Label Etiqueta(string texto, int y) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location  = new Point(10, y + 2),
            Size      = new Size(118, 22),
            TextAlign = ContentAlignment.MiddleLeft,
        };

        private static TextBox Campo(int y, int ancho) => new TextBox
        {
            Font     = new Font("Segoe UI", 10F),
            Location = new Point(130, y),
            Size     = new Size(ancho, 26),
        };

        private static ComboBox HoraCombo(int x, int desde, int hasta)
        {
            var cmb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10F),
                Location      = new Point(x, 215),
                Size          = new Size(82, 26),
            };
            for (int h = desde; h <= hasta; h++)
                cmb.Items.Add(h == 24 ? "24:00" : $"{h}:00");
            return cmb;
        }

        // ── Carga de la lista ──────────────────────────────────────────────────

        private void CargarLista()
        {
            var selCodigo = _lista.SelectedItems.Count > 0 ? _lista.SelectedItems[0].Text : null;
            _lista.Items.Clear();

            foreach (var t in _storage.GetTarifasRegistradas())
            {
                var item = new ListViewItem(t.Codigo);
                item.SubItems.Add(t.Nombre);
                item.SubItems.Add(t.Tipo == TipoTarifa.PorHora ? "Por hora" : "Por día");
                item.SubItems.Add(t.AplicaA.HasValue ? t.AplicaA.Value.ToString() : "Todos");
                item.SubItems.Add($"{t.HoraInicio}:00–{t.HoraFin}:00");
                item.SubItems.Add($"${t.Valor:0.##}");
                item.Tag = t;
                _lista.Items.Add(item);

                if (item.Text == selCodigo)
                    item.Selected = true;
            }
        }

        // ── Eventos ───────────────────────────────────────────────────────────

        private void OnSeleccionCambiada(object sender, EventArgs e)
        {
            if (_lista.SelectedItems.Count == 0)
            {
                _btnEliminar.Enabled = false;
                return;
            }

            var t = (Tarifa)_lista.SelectedItems[0].Tag;
            _txtCodigo.Text           = t.Codigo;
            _txtNombre.Text           = t.Nombre;
            _cmbTipo.SelectedIndex    = (int)t.Tipo;
            _cmbAplicaA.SelectedIndex = t.AplicaA.HasValue ? (int)t.AplicaA.Value + 1 : 0;
            _txtValor.Text            = t.Valor.ToString("0.##");
            _cmbHoraInicio.SelectedIndex = t.HoraInicio;           // índice == hora
            _cmbHoraFin.SelectedIndex    = t.HoraFin - 1;          // índice == hora - 1
            _txtDescripcion.Text      = t.Descripcion;
            _btnEliminar.Enabled      = true;
        }

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            string codigo = _txtCodigo.Text.Trim();
            if (codigo.Length == 0 || Encoding.UTF8.GetByteCount(codigo) > 8)
            {
                Aviso("El código debe tener entre 1 y 8 caracteres ASCII."); return;
            }

            string nombre = _txtNombre.Text.Trim();
            if (nombre.Length == 0) { Aviso("Ingrese un nombre para la tarifa."); return; }

            if (!double.TryParse(_txtValor.Text.Trim().Replace(',', '.'),
                                 System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out double valor) || valor <= 0)
            {
                Aviso("El valor debe ser un número mayor a cero."); return;
            }

            int horaInicio = _cmbHoraInicio.SelectedIndex;      // 0-23
            int horaFin    = _cmbHoraFin.SelectedIndex + 1;     // 1-24

            if (horaFin <= horaInicio)
            {
                Aviso("La hora de fin debe ser posterior a la hora de inicio."); return;
            }

            var tipo    = (TipoTarifa)_cmbTipo.SelectedIndex;
            var aplicaA = _cmbAplicaA.SelectedIndex == 0
                          ? (TipoVehiculo?)null
                          : (TipoVehiculo)(_cmbAplicaA.SelectedIndex - 1);

            var nueva = new Tarifa(codigo, nombre, tipo, aplicaA, valor,
                                   horaInicio, horaFin, _txtDescripcion.Text.Trim());

            string conflicto = ValidarSuperposicion(nueva);
            if (conflicto != null)
            {
                MessageBox.Show(
                    $"No se puede crear la tarifa porque se superpone con una existente:\n\n{conflicto}",
                    "Superposición de tarifas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_storage.AddTarifa(nueva))
            {
                Aviso($"Ya existe una tarifa con el código «{codigo}». Elija otro código."); return;
            }

            CargarLista();
            LimpiarFormulario();

            // Resaltar la tarifa recién creada en la lista.
            foreach (ListViewItem item in _lista.Items)
            {
                if (item.Text != codigo) continue;
                item.Selected = true;
                item.EnsureVisible();
                break;
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_lista.SelectedItems.Count == 0) return;
            var t = (Tarifa)_lista.SelectedItems[0].Tag;

            if (MessageBox.Show(
                    $"¿Eliminar la tarifa «{t.Nombre}» ({t.Codigo})?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;

            _storage.DeleteTarifa(t.Codigo);
            CargarLista();
            LimpiarFormulario();
        }

        // ── Lógica de validación ───────────────────────────────────────────────

        // Devuelve null si no hay conflictos, o un mensaje descriptivo si los hay.
        // Dos tarifas se superponen cuando tienen el mismo tipo (PorHora / PorDia),
        // exactamente el mismo AplicaA (null==null, Carro==Carro), y sus rangos se intersectan.
        private string ValidarSuperposicion(Tarifa nueva)
        {
            foreach (var e in _storage.GetTarifasRegistradas())
            {
                if (nueva.Tipo != e.Tipo) continue;

                bool mismoVehiculo = nueva.AplicaA == e.AplicaA;
                if (!mismoVehiculo) continue;

                // Intervalos semi-abiertos [inicio, fin) se intersectan si inicio1 < fin2 && inicio2 < fin1.
                bool rangoSolapa = nueva.HoraInicio < e.HoraFin && e.HoraInicio < nueva.HoraFin;
                if (!rangoSolapa) continue;

                string vehiculo = e.AplicaA.HasValue ? e.AplicaA.Value.ToString() : "todos los vehículos";
                return $"«{e.Nombre}» ({e.Codigo})  ·  {vehiculo}  ·  " +
                       $"{e.HoraInicio}:00 – {e.HoraFin}:00";
            }
            return null;
        }

        private void LimpiarFormulario()
        {
            _txtCodigo.Text           = string.Empty;
            _txtNombre.Text           = string.Empty;
            _cmbTipo.SelectedIndex    = 0;
            _cmbAplicaA.SelectedIndex = 0;
            _txtValor.Text            = string.Empty;
            _cmbHoraInicio.SelectedIndex = 8;   // 8:00
            _cmbHoraFin.SelectedIndex    = 17;  // 18:00
            _txtDescripcion.Text      = string.Empty;
            _btnEliminar.Enabled      = false;
            _lista.SelectedItems.Clear();
        }

        private static void Aviso(string msg) =>
            MessageBox.Show(msg, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // Restaurar directorio de trabajo al cerrar para que Form1 siga operando.
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Directory.SetCurrentDirectory(_returnDir);
            base.OnFormClosed(e);
        }
    }
}

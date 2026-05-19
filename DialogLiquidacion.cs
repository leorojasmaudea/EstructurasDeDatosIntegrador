using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EstructurasDeDatosIntegrador.Storage;

namespace EstructurasDeDatosIntegrador
{
    internal class DialogLiquidacion : Form
    {
        public DialogLiquidacion(Vehiculo vehiculo)
        {
            DateTime   salida   = DateTime.Now;
            TimeSpan   duracion = salida - vehiculo.HoraEntrada;
            Tarifa     tarifa   = BuscarTarifa(vehiculo, salida);

            double unidades     = 0;
            string unidadNombre = "";
            double costoTotal   = 0;

            if (tarifa != null)
            {
                if (vehiculo.Tarifa == TipoTarifa.PorHora)
                {
                    unidades     = Math.Max(1, Math.Ceiling(duracion.TotalHours));
                    unidadNombre = unidades == 1 ? "hora" : "horas";
                }
                else
                {
                    unidades     = Math.Max(1, Math.Ceiling(duracion.TotalDays));
                    unidadNombre = unidades == 1 ? "día" : "días";
                }
                costoTotal = unidades * tarifa.Valor;
            }

            // ── Propiedades del formulario ────────────────────────────────────
            Text            = "Liquidación de Servicio";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(240, 242, 245);

            // ── Encabezado ────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                BackColor = Color.FromArgb(30, 47, 78),
                Location  = new Point(0, 0),
                Size      = new Size(440, 70),
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "LIQUIDACIÓN DE SERVICIO",
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(16, 8),
                Size      = new Size(408, 24),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = vehiculo.Placa,
                Font      = new Font("Courier New", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 214, 0),
                Location  = new Point(16, 34),
                Size      = new Size(408, 28),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            Controls.Add(pnlHeader);

            // ── Cuerpo del formulario ─────────────────────────────────────────
            int y = 82;

            // Sección: datos del vehículo
            Fila("Tipo de vehículo:",   vehiculo.Tipo.ToString(), ref y);
            Fila("Modalidad de cobro:", vehiculo.Tarifa == TipoTarifa.PorHora ? "Por hora" : "Por día", ref y);

            Separador(y); y += 14;

            // Sección: tiempos
            Fila("Hora de entrada:", vehiculo.HoraEntrada.ToString("dd/MM/yyyy   HH:mm:ss"), ref y);
            Fila("Hora de salida:",  salida.ToString("dd/MM/yyyy   HH:mm:ss"),               ref y);

            string durStr = duracion.TotalHours >= 1
                ? $"{(int)duracion.TotalHours}h {duracion.Minutes:D2}min"
                : $"{duracion.Minutes}min";
            string unidStr = tarifa != null ? $"  →  {(int)unidades} {unidadNombre} facturadas" : "";
            Fila("Duración:", durStr + unidStr, ref y);

            Separador(y); y += 14;

            // Sección: tarifa
            if (tarifa != null)
            {
                string unidLabel = vehiculo.Tarifa == TipoTarifa.PorHora ? "Valor por hora:" : "Valor por día:";
                Fila("Tarifa aplicada:", $"{tarifa.Nombre}  ({tarifa.Codigo})", ref y);
                Fila(unidLabel,          $"${tarifa.Valor:N0}",                 ref y);
                Fila("Unidades:",        $"{(int)unidades} {unidadNombre}",     ref y);
            }
            else
            {
                Controls.Add(new Label
                {
                    Text      = "Sin tarifa configurada para este vehículo y horario.",
                    Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(160, 90, 0),
                    Location  = new Point(16, y),
                    Size      = new Size(408, 22),
                });
                y += 26;
            }

            y += 8;

            // ── Panel de total ────────────────────────────────────────────────
            var pnlTotal = new Panel
            {
                BackColor = Color.FromArgb(30, 47, 78),
                Location  = new Point(16, y),
                Size      = new Size(408, 56),
            };
            pnlTotal.Controls.Add(new Label
            {
                Text      = "TOTAL A PAGAR",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 210, 255),
                Location  = new Point(14, 0),
                Size      = new Size(180, 56),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            pnlTotal.Controls.Add(new Label
            {
                Text      = $"$ {costoTotal:N0}",
                Font      = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 214, 0),
                Location  = new Point(160, 0),
                Size      = new Size(234, 56),
                TextAlign = ContentAlignment.MiddleRight,
            });
            Controls.Add(pnlTotal);
            y += 68;

            // ── Botones ───────────────────────────────────────────────────────
            var btnCancelar = new Button
            {
                Text         = "Cancelar",
                Font         = new Font("Segoe UI", 10F),
                BackColor    = Color.White,
                ForeColor    = Color.FromArgb(80, 80, 80),
                FlatStyle    = FlatStyle.Flat,
                Location     = new Point(16, y),
                Size         = new Size(120, 38),
                DialogResult = DialogResult.Cancel,
                Cursor       = Cursors.Hand,
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);

            var btnConfirmar = new Button
            {
                Text         = "CONFIRMAR SALIDA",
                Font         = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor    = Color.FromArgb(160, 30, 30),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat,
                Location     = new Point(148, y),
                Size         = new Size(276, 38),
                DialogResult = DialogResult.OK,
                Cursor       = Cursors.Hand,
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;

            CancelButton = btnCancelar;
            AcceptButton = btnConfirmar;
            Controls.Add(btnCancelar);
            Controls.Add(btnConfirmar);

            ClientSize = new Size(440, y + 38 + 14);
        }

        // ── Helpers de layout ─────────────────────────────────────────────────

        private void Fila(string etiqueta, string valor, ref int y)
        {
            Controls.Add(new Label
            {
                Text      = etiqueta,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location  = new Point(16, y),
                Size      = new Size(168, 22),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            Controls.Add(new Label
            {
                Text      = valor,
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(188, y),
                Size      = new Size(236, 22),
                TextAlign = ContentAlignment.MiddleLeft,
            });
            y += 26;
        }

        private void Separador(int y)
        {
            Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(200, 200, 210),
                Location  = new Point(16, y + 2),
                Size      = new Size(408, 1),
            });
        }

        // ── Búsqueda de tarifa aplicable ──────────────────────────────────────

        private static Tarifa BuscarTarifa(Vehiculo vehiculo, DateTime horaRef)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string tarifsDir  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TarifsData");
            if (!Directory.Exists(tarifsDir)) return null;

            try
            {
                Directory.SetCurrentDirectory(tarifsDir);
                var storage = new HashingStorageTarifas();
                storage.EnsureInitialized();

                int    hora  = horaRef.Hour;
                Tarifa mejor = null;

                foreach (var t in storage.GetTarifasRegistradas())
                {
                    if (t.Tipo != vehiculo.Tarifa) continue;

                    bool aplicaVehiculo = !t.AplicaA.HasValue || t.AplicaA.Value == vehiculo.Tipo;
                    if (!aplicaVehiculo) continue;

                    bool enRango = hora >= t.HoraInicio && hora < t.HoraFin;
                    if (!enRango) continue;

                    // Preferir tarifa específica de vehículo sobre la genérica "todos".
                    if (mejor == null || (t.AplicaA.HasValue && !mejor.AplicaA.HasValue))
                        mejor = t;
                }
                return mejor;
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }
        }
    }
}

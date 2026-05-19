namespace EstructurasDeDatosIntegrador
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader       = new System.Windows.Forms.Panel();
            this.lblTitulo       = new System.Windows.Forms.Label();
            this.lblHora         = new System.Windows.Forms.Label();
            this.grpCapacidad    = new System.Windows.Forms.GroupBox();
            this.lblCapTotalTxt  = new System.Windows.Forms.Label();
            this.lblCapTotal     = new System.Windows.Forms.Label();
            this.lblOcupadosTxt  = new System.Windows.Forms.Label();
            this.lblOcupados     = new System.Windows.Forms.Label();
            this.lblDispTxt      = new System.Windows.Forms.Label();
            this.lblDisponibles  = new System.Windows.Forms.Label();
            this.grpRegistro     = new System.Windows.Forms.GroupBox();
            this.lblPlacaHeader  = new System.Windows.Forms.Label();
            this.txtPlaca        = new System.Windows.Forms.TextBox();
            this.lblPlacaAyuda   = new System.Windows.Forms.Label();
            this.btnRegistrar    = new System.Windows.Forms.Button();
            this.btnVerTodos     = new System.Windows.Forms.Button();
            this.btnTarifas      = new System.Windows.Forms.Button();
            this.grpCamara       = new System.Windows.Forms.GroupBox();
            this.picCamara       = new System.Windows.Forms.PictureBox();
            this.timerReloj      = new System.Windows.Forms.Timer(this.components);

            this.pnlHeader.SuspendLayout();
            this.grpCapacidad.SuspendLayout();
            this.grpRegistro.SuspendLayout();
            this.grpCamara.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamara)).BeginInit();
            this.SuspendLayout();

            // ── pnlHeader ──────────────────────────────────────────────────────
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 47, 78);
            this.pnlHeader.Controls.Add(this.lblHora);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 80;
            this.pnlHeader.TabIndex = 0;

            // ── lblTitulo ──────────────────────────────────────────────────────
            this.lblTitulo.AutoSize = false;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(18, 15);
            this.lblTitulo.Size = new System.Drawing.Size(420, 50);
            this.lblTitulo.Text = "PARQUEADERO";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ── lblHora ────────────────────────────────────────────────────────
            this.lblHora.AutoSize = false;
            this.lblHora.Font = new System.Drawing.Font("Courier New", 36F, System.Drawing.FontStyle.Bold);
            this.lblHora.ForeColor = System.Drawing.Color.FromArgb(255, 214, 0);
            this.lblHora.Location = new System.Drawing.Point(580, 8);
            this.lblHora.Size = new System.Drawing.Size(505, 64);
            this.lblHora.Text = "00:00:00";
            this.lblHora.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ── grpCapacidad ───────────────────────────────────────────────────
            this.grpCapacidad.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpCapacidad.Location = new System.Drawing.Point(10, 90);
            this.grpCapacidad.Size = new System.Drawing.Size(435, 148);
            this.grpCapacidad.TabIndex = 1;
            this.grpCapacidad.Text = "CAPACIDAD DEL PARQUEADERO";
            this.grpCapacidad.Controls.Add(this.lblCapTotalTxt);
            this.grpCapacidad.Controls.Add(this.lblCapTotal);
            this.grpCapacidad.Controls.Add(this.lblOcupadosTxt);
            this.grpCapacidad.Controls.Add(this.lblOcupados);
            this.grpCapacidad.Controls.Add(this.lblDispTxt);
            this.grpCapacidad.Controls.Add(this.lblDisponibles);

            // Fila 1 – capacidad total
            this.lblCapTotalTxt.AutoSize = false;
            this.lblCapTotalTxt.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblCapTotalTxt.Location = new System.Drawing.Point(15, 30);
            this.lblCapTotalTxt.Size = new System.Drawing.Size(215, 28);
            this.lblCapTotalTxt.Text = "Capacidad total:";
            this.lblCapTotalTxt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblCapTotal.AutoSize = false;
            this.lblCapTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCapTotal.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblCapTotal.ForeColor = System.Drawing.Color.FromArgb(30, 47, 78);
            this.lblCapTotal.Location = new System.Drawing.Point(238, 28);
            this.lblCapTotal.Size = new System.Drawing.Size(64, 32);
            this.lblCapTotal.Text = "0";
            this.lblCapTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Fila 2 – vehículos presentes
            this.lblOcupadosTxt.AutoSize = false;
            this.lblOcupadosTxt.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblOcupadosTxt.Location = new System.Drawing.Point(15, 65);
            this.lblOcupadosTxt.Size = new System.Drawing.Size(215, 28);
            this.lblOcupadosTxt.Text = "Vehículos presentes:";
            this.lblOcupadosTxt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblOcupados.AutoSize = false;
            this.lblOcupados.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOcupados.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblOcupados.ForeColor = System.Drawing.Color.Crimson;
            this.lblOcupados.Location = new System.Drawing.Point(238, 63);
            this.lblOcupados.Size = new System.Drawing.Size(64, 32);
            this.lblOcupados.Text = "0";
            this.lblOcupados.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Fila 3 – disponibles
            this.lblDispTxt.AutoSize = false;
            this.lblDispTxt.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDispTxt.Location = new System.Drawing.Point(15, 100);
            this.lblDispTxt.Size = new System.Drawing.Size(215, 28);
            this.lblDispTxt.Text = "Disponibles:";
            this.lblDispTxt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblDisponibles.AutoSize = false;
            this.lblDisponibles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDisponibles.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblDisponibles.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblDisponibles.Location = new System.Drawing.Point(238, 98);
            this.lblDisponibles.Size = new System.Drawing.Size(64, 32);
            this.lblDisponibles.Text = "0";
            this.lblDisponibles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ── grpRegistro ────────────────────────────────────────────────────
            this.grpRegistro.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpRegistro.Location = new System.Drawing.Point(10, 248);
            this.grpRegistro.Size = new System.Drawing.Size(435, 340);
            this.grpRegistro.TabIndex = 2;
            this.grpRegistro.Text = "REGISTRO DE ENTRADA";
            this.grpRegistro.Controls.Add(this.lblPlacaHeader);
            this.grpRegistro.Controls.Add(this.txtPlaca);
            this.grpRegistro.Controls.Add(this.lblPlacaAyuda);
            this.grpRegistro.Controls.Add(this.btnRegistrar);
            this.grpRegistro.Controls.Add(this.btnVerTodos);
            this.grpRegistro.Controls.Add(this.btnTarifas);

            // Instrucción
            this.lblPlacaHeader.AutoSize = false;
            this.lblPlacaHeader.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblPlacaHeader.Location = new System.Drawing.Point(10, 28);
            this.lblPlacaHeader.Size = new System.Drawing.Size(410, 28);
            this.lblPlacaHeader.Text = "Ingrese la placa del vehículo";
            this.lblPlacaHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Campo de placa – fuente grande y centrado
            this.txtPlaca.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtPlaca.Font = new System.Drawing.Font("Courier New", 32F, System.Drawing.FontStyle.Bold);
            this.txtPlaca.Location = new System.Drawing.Point(18, 65);
            this.txtPlaca.MaxLength = 7;
            this.txtPlaca.Multiline = true;
            this.txtPlaca.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtPlaca.Size = new System.Drawing.Size(396, 72);
            this.txtPlaca.TabIndex = 0;
            this.txtPlaca.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtPlaca.KeyPress     += new System.Windows.Forms.KeyPressEventHandler(this.txtPlaca_KeyPress);
            this.txtPlaca.TextChanged  += new System.EventHandler(this.txtPlaca_TextChanged);

            // Texto de ayuda bajo el campo
            this.lblPlacaAyuda.AutoSize = false;
            this.lblPlacaAyuda.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPlacaAyuda.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblPlacaAyuda.Location = new System.Drawing.Point(10, 146);
            this.lblPlacaAyuda.Size = new System.Drawing.Size(410, 22);
            this.lblPlacaAyuda.Text = "Ejemplo:  ABC123  ·  XYZ45D";
            this.lblPlacaAyuda.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Botón de registro
            this.btnRegistrar.BackColor = System.Drawing.Color.FromArgb(30, 47, 78);
            this.btnRegistrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegistrar.FlatAppearance.BorderSize = 0;
            this.btnRegistrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistrar.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnRegistrar.ForeColor = System.Drawing.Color.White;
            this.btnRegistrar.Location = new System.Drawing.Point(68, 188);
            this.btnRegistrar.Size = new System.Drawing.Size(295, 55);
            this.btnRegistrar.TabIndex = 1;
            this.btnRegistrar.Text = "REGISTRAR ENTRADA";
            this.btnRegistrar.UseVisualStyleBackColor = false;
            this.btnRegistrar.Click += new System.EventHandler(this.btnRegistrar_Click);

            // Botón secundario — ver todos los vehículos
            this.btnVerTodos.BackColor    = System.Drawing.Color.White;
            this.btnVerTodos.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnVerTodos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(30, 47, 78);
            this.btnVerTodos.FlatAppearance.BorderSize  = 1;
            this.btnVerTodos.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerTodos.Font         = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVerTodos.ForeColor    = System.Drawing.Color.FromArgb(30, 47, 78);
            this.btnVerTodos.Location     = new System.Drawing.Point(68, 253);
            this.btnVerTodos.Size         = new System.Drawing.Size(295, 36);
            this.btnVerTodos.TabIndex     = 2;
            this.btnVerTodos.Text         = "VER TODOS LOS VEHÍCULOS";
            this.btnVerTodos.UseVisualStyleBackColor = false;
            this.btnVerTodos.Click += new System.EventHandler(this.btnVerTodos_Click);

            // Botón de administración de tarifas
            this.btnTarifas.BackColor    = System.Drawing.Color.FromArgb(90, 60, 20);
            this.btnTarifas.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnTarifas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 120, 40);
            this.btnTarifas.FlatAppearance.BorderSize  = 1;
            this.btnTarifas.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnTarifas.Font         = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTarifas.ForeColor    = System.Drawing.Color.White;
            this.btnTarifas.Location     = new System.Drawing.Point(68, 297);
            this.btnTarifas.Size         = new System.Drawing.Size(295, 34);
            this.btnTarifas.TabIndex     = 3;
            this.btnTarifas.Text         = "ADMINISTRAR TARIFAS";
            this.btnTarifas.UseVisualStyleBackColor = false;
            this.btnTarifas.Click += new System.EventHandler(this.btnTarifas_Click);

            // ── grpCamara ──────────────────────────────────────────────────────
            this.grpCamara.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpCamara.Location = new System.Drawing.Point(455, 90);
            this.grpCamara.Size = new System.Drawing.Size(640, 458);
            this.grpCamara.TabIndex = 3;
            this.grpCamara.Text = "CÁMARA EN VIVO";
            this.grpCamara.Controls.Add(this.picCamara);

            // PictureBox para el video
            this.picCamara.BackColor = System.Drawing.Color.FromArgb(18, 18, 18);
            this.picCamara.Location = new System.Drawing.Point(8, 22);
            this.picCamara.Size = new System.Drawing.Size(622, 425);
            this.picCamara.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCamara.TabIndex = 0;
            this.picCamara.TabStop = false;
            this.picCamara.Paint += new System.Windows.Forms.PaintEventHandler(this.picCamara_Paint);

            // ── timerReloj ─────────────────────────────────────────────────────
            this.timerReloj.Enabled = true;
            this.timerReloj.Interval = 1000;
            this.timerReloj.Tick += new System.EventHandler(this.timerReloj_Tick);

            // ── Form1 ──────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(1105, 600);
            this.Controls.Add(this.grpCamara);
            this.Controls.Add(this.grpRegistro);
            this.Controls.Add(this.grpCapacidad);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema de Parqueadero";

            this.pnlHeader.ResumeLayout(false);
            this.grpCapacidad.ResumeLayout(false);
            this.grpRegistro.ResumeLayout(false);
            this.grpCamara.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCamara)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblTitulo;
        private System.Windows.Forms.Label      lblHora;
        private System.Windows.Forms.GroupBox   grpCapacidad;
        private System.Windows.Forms.Label      lblCapTotalTxt;
        private System.Windows.Forms.Label      lblCapTotal;
        private System.Windows.Forms.Label      lblOcupadosTxt;
        private System.Windows.Forms.Label      lblOcupados;
        private System.Windows.Forms.Label      lblDispTxt;
        private System.Windows.Forms.Label      lblDisponibles;
        private System.Windows.Forms.GroupBox   grpRegistro;
        private System.Windows.Forms.Label      lblPlacaHeader;
        private System.Windows.Forms.TextBox    txtPlaca;
        private System.Windows.Forms.Label      lblPlacaAyuda;
        private System.Windows.Forms.Button     btnRegistrar;
        private System.Windows.Forms.Button     btnVerTodos;
        private System.Windows.Forms.Button     btnTarifas;
        private System.Windows.Forms.GroupBox   grpCamara;
        private System.Windows.Forms.PictureBox picCamara;
        private System.Windows.Forms.Timer      timerReloj;
    }
}

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace lab_19
{

    public interface IDrawingMethod
    {
        string Name { get; }
        void Render(Shape shape, Graphics g);
    }

    public abstract class Shape
    {
        protected IDrawingMethod drawingMethod; 

        protected Shape(IDrawingMethod method)
        {
            drawingMethod = method;
        }

        public void Draw(Graphics g) => drawingMethod.Render(this, g);

        public abstract void DrawContour(Graphics g, Pen pen);
        public abstract void Fill(Graphics g, Brush brush);
        public abstract string GetDescription();
    }

    public class Circle : Shape
    {
        public float X { get; }
        public float Y { get; }
        public float Radius { get; }

        public Circle(IDrawingMethod method, float x, float y, float radius) : base(method)
        {
            X = x; Y = y; Radius = radius;
        }

        public override void DrawContour(Graphics g, Pen pen) =>
            g.DrawEllipse(pen, X - Radius, Y - Radius, Radius * 2, Radius * 2);

        public override void Fill(Graphics g, Brush brush) =>
            g.FillEllipse(brush, X - Radius, Y - Radius, Radius * 2, Radius * 2);

        public override string GetDescription() =>
            $"[Круг] Центр=({X}; {Y}), R={Radius} | Способ: {drawingMethod.Name}";
    }

    public class Square : Shape
    {
        public float X { get; }
        public float Y { get; }
        public float Side { get; }

        public Square(IDrawingMethod method, float x, float y, float side) : base(method)
        {
            X = x; Y = y; Side = side;
        }

        public override void DrawContour(Graphics g, Pen pen) =>
            g.DrawRectangle(pen, X, Y, Side, Side);

        public override void Fill(Graphics g, Brush brush) =>
            g.FillRectangle(brush, X, Y, Side, Side);

        public override string GetDescription() =>
            $"[Квадрат] X={X}, Y={Y}, Сторона={Side} | Способ: {drawingMethod.Name}";
    }

    public class Triangle : Shape
    {
        public PointF[] Points { get; }

        public Triangle(IDrawingMethod method, PointF p1, PointF p2, PointF p3) : base(method)
        {
            Points = new[] { p1, p2, p3 };
        }

        public override void DrawContour(Graphics g, Pen pen) =>
            g.DrawPolygon(pen, Points);

        public override void Fill(Graphics g, Brush brush) =>
            g.FillPolygon(brush, Points);

        public override string GetDescription() =>
            $"[Треугольник] 3 вершины | Способ: {drawingMethod.Name}";
    }


    public class VectorDrawingMethod : IDrawingMethod
    {
        public string Name => "Векторный";

        public void Render(Shape shape, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias; 
            using var pen = new Pen(Color.RoyalBlue, 3); // using чтобы не было утечек памчти
            shape.DrawContour(g, pen);
        }
    }

    public class RasterDrawingMethod : IDrawingMethod
    {
        public string Name => "Растровый";

        public void Render(Shape shape, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.None; 
            using var brush = new HatchBrush(HatchStyle.Percent50, Color.Crimson, Color.DarkRed);
            using var pen = new Pen(Color.DarkRed, 3);
            shape.Fill(g, brush);
            shape.DrawContour(g, pen);
        }
    }

 
    public class MainForm : Form
    {
        private ComboBox cmbShapeType;
        private ComboBox cmbDrawingMethod;
        private Button btnDraw;
        private Button btnClear;
        private PictureBox canvasBox;
        private ListBox logListBox;
        private Bitmap canvasBitmap;

        public MainForm()
        {
            Text = "Фигурки";
            Size = new Size(820, 580);
            StartPosition = FormStartPosition.CenterScreen;


            var panel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.WhiteSmoke };

            cmbShapeType = new ComboBox { Location = new Point(15, 25), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbShapeType.Items.AddRange(new object[] { "Круг", "Квадрат", "Треугольник" });
            cmbShapeType.SelectedIndex = 0;

            cmbDrawingMethod = new ComboBox { Location = new Point(180, 25), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDrawingMethod.Items.AddRange(new object[] { "Векторный", "Растровый" });
            cmbDrawingMethod.SelectedIndex = 0;

            btnDraw = new Button { Text = "Нарисовать", Location = new Point(345, 23), Size = new Size(110, 30), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnDraw.Click += BtnDraw_Click;

            btnClear = new Button { Text = "Очистить", Location = new Point(465, 23), Size = new Size(100, 30), BackColor = Color.MistyRose, FlatStyle = FlatStyle.Flat };
            btnClear.Click += (s, e) => ClearCanvas();

            panel.Controls.AddRange(new Control[] { cmbShapeType, cmbDrawingMethod, btnDraw, btnClear });

            logListBox = new ListBox { Dock = DockStyle.Bottom, Height = 130, Font = new Font("Consolas", 9.5f) };

            canvasBox = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };
            canvasBox.SizeChanged += (s, e) => ClearCanvas();

            Controls.AddRange(new Control[] { canvasBox, logListBox, panel });
            ClearCanvas();
        }

        private void ClearCanvas()
        {
            if (canvasBox.Width <= 0 || canvasBox.Height <= 0) return;
            canvasBitmap = new Bitmap(canvasBox.Width, canvasBox.Height);
            using (var g = Graphics.FromImage(canvasBitmap)) g.Clear(Color.White);
            canvasBox.Image = canvasBitmap;
            logListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] Холст очищен.");
        }

        private void BtnDraw_Click(object sender, EventArgs e)
        {

            IDrawingMethod method = cmbDrawingMethod.SelectedIndex == 0 ? new VectorDrawingMethod() : new RasterDrawingMethod();

            var rnd = new Random();
            int cx = rnd.Next(100, Math.Max(120, canvasBox.Width - 100));
            int cy = rnd.Next(100, Math.Max(120, canvasBox.Height - 100));

            Shape shape;

            switch (cmbShapeType.SelectedIndex)
            {
                case 0:
                    shape = new Circle(method, cx, cy, 45);
                    break;
                case 1:
                    shape = new Square(method, cx - 40, cy - 40, 80);
                    break;
                case 2:
                    shape = new Triangle(method, new PointF(cx, cy - 45), new PointF(cx - 45, cy + 40), new PointF(cx + 45, cy + 40));
                    break;
                default:
                    shape = new Circle(method, cx, cy, 45);
                    break;
            }

            using (var g = Graphics.FromImage(canvasBitmap))
            {
                shape.Draw(g);
            }
            canvasBox.Invalidate();

            logListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {shape.GetDescription()}");
            logListBox.SelectedIndex = logListBox.Items.Count - 1;
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
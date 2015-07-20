using System.Drawing;
using System.Linq;

namespace Doomnet
{
    class LevelDrawer
    {
            Pen penNormal = new Pen(Color.Black);
            Pen penImpass = new Pen(Color.Black, 3);
            Pen penSecret = new Pen(Color.DarkGreen);
            Pen penInvis = new Pen(Color.Gray);
            Pen penTwoside = new Pen(Color.Red);

        public void SaveImage(Level level)
        {
            var width = level.Vertices.Max(v => v.X) + 1;
            var height = level.Vertices.Max(v => v.X) + 1;
            var bitmap = new Bitmap(width, height);

            foreach (var vertex in level.Vertices)
            {
                bitmap.SetPixel(vertex.X, vertex.Y, Color.Black);
            }

            using (var graphics = Graphics.FromImage(bitmap))
            {
                DrawLines(level, graphics);

                DrawThings(level, graphics);
            }

            // bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            bitmap.Save(level.Definition.Name.Trim() + ".png");
        }

        private void DrawThings(Level level, Graphics graphics)
        {
            foreach (var thing in level.Things)
            {
                graphics.DrawLine(Pens.Red, thing.posX - 5, thing.posY - 5, thing.posX + 5, thing.posY + 5);
                graphics.DrawLine(Pens.Red, thing.posX - 5, thing.posY + 5, thing.posX + 5, thing.posY - 5);
            }
        }

        private void DrawLines(Level level, Graphics graphics)
        {
            foreach (var segment in level.Segments)
            {
                var line = level.Linedefs.FirstOrDefault(l => (segment.start == l.start && segment.end == l.end)
                                                              || (segment.end == l.start && segment.start == l.end));

                Pen pen;
                pen = SelectLinePen(line);
                graphics.DrawLine(pen, segment.start.X, segment.start.Y, segment.end.X, segment.end.Y);
            }
        }

        private Pen SelectLinePen(Linedef line)
        {
            Pen pen;
            if (line != null)
            {
                switch (line.flags)
                {
                    case Linedef.Flags.Secret:
                        pen = penSecret;
                        break;
                    case Linedef.Flags.NotOnMap:
                        pen = penInvis;
                        break;
                    case Linedef.Flags.Impassible:
                        pen = penImpass;
                        break;
                    case Linedef.Flags.TwoSided:
                        pen = penTwoside;
                        break;
                    default:
                        pen = penNormal;
                        break;
                }
                if (line.left == null)
                    pen = penImpass;
            }
            else
            {
                pen = penNormal;
            }
            return pen;
        }
    }
}
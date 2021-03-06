using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenRA.TilesetBuilder
{
	class Surface : Control
	{
		public Bitmap Image;
		public int[,] TerrainTypes;
		public List<Template> Templates = new List<Template>();
		public string InputMode;
		public Bitmap[] Icon;
		public int TileSize;
		public int TilesPerRow;

		public event Action<int, int, int> UpdateMouseTilePosition = (x, y, t) => { };

		Template currentTemplate;
		ImageList imagesListControl;
		bool showTerrainTypes;

		public bool ShowTerrainTypes
		{
			get { return showTerrainTypes; }
			set { showTerrainTypes = value; }
		}

		public ImageList ImagesList
		{
			get { return imagesListControl; }
			set { imagesListControl = value; }
		}

		public Surface()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			UpdateStyles();
		}

		Brush currentBrush = new SolidBrush(Color.FromArgb(60, Color.White));

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Image == null || TerrainTypes == null || Templates == null)
				return;

			/* draw the background */
			e.Graphics.DrawImage(Image, 0, 0, Image.Width, Image.Height);
			/* draw terrain type overlays */
			if (ShowTerrainTypes)
			{
				for (var i = 0; i <= TerrainTypes.GetUpperBound(0); i++)
					for (var j = 0; j <= TerrainTypes.GetUpperBound(1); j++)
						if (TerrainTypes[i, j] != 0)
						{
							////e.Graphics.FillRectangle(Brushes.Black, TileSize * i + 8, TileSize * j + 8, 16, 16);

							e.Graphics.DrawImage(Icon[TerrainTypes[i, j]], TileSize * i + 8, TileSize * j + 8, 16, 16);

							////e.Graphics.DrawString(TerrainTypes[i, j].ToString(),
							////Font, Brushes.LimeGreen, TileSize * i + 10, TileSize * j + 10);
						}
			}

			/* draw template outlines */
			foreach (var t in Templates)
			{
				var pen = Pens.White;

				foreach (var c in t.Cells.Keys)
				{
					if (currentTemplate == t)
						e.Graphics.FillRectangle(currentBrush, TileSize * c.X, TileSize * c.Y, TileSize, TileSize);

					if (!t.Cells.ContainsKey(c + new int2(-1, 0)))
					{
						var a = TileSize * c;
						var b = TileSize * (c + new int2(0, 1));
						e.Graphics.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
					}

					if (!t.Cells.ContainsKey(c + new int2(+1, 0)))
					{
						var a = TileSize * (c + new int2(1, 0));
						var b = TileSize * (c + new int2(1, 1));
						e.Graphics.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
					}

					if (!t.Cells.ContainsKey(c + new int2(0, +1)))
					{
						var a = TileSize * (c + new int2(0, 1));
						var b = TileSize * (c + new int2(1, 1));
						e.Graphics.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
					}

					if (!t.Cells.ContainsKey(c + new int2(0, -1)))
					{
						var a = TileSize * c;
						var b = TileSize * (c + new int2(1, 0));
						e.Graphics.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
					}
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			var pos = new int2(e.X / TileSize, e.Y / TileSize);

			if (InputMode == null)
			{
				if (e.Button == MouseButtons.Left)
				{
					currentTemplate = Templates.FirstOrDefault(t => t.Cells.ContainsKey(pos));
					if (currentTemplate == null)
						Templates.Add(currentTemplate = new Template { Cells = new Dictionary<int2, bool> { { pos, true } } });

					Invalidate();
				}

				if (e.Button == MouseButtons.Right)
				{
					Templates.RemoveAll(t => t.Cells.ContainsKey(pos));
					currentTemplate = null;
					Invalidate();
				}
			}
			else
			{
				TerrainTypes[pos.X, pos.Y] = int.Parse(InputMode);
				Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			var pos = new int2(e.X / TileSize, e.Y / TileSize);

			if (InputMode == null)
			{
				if (e.Button == MouseButtons.Left && currentTemplate != null)
				{
					if (!currentTemplate.Cells.ContainsKey(pos))
					{
						currentTemplate.Cells[pos] = true;
						Invalidate();
					}
				}
			}

			UpdateMouseTilePosition(pos.X, pos.Y, (pos.Y * TilesPerRow) + pos.X);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ResumeLayout(false);
		}
	}
}

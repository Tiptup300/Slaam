using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlaamMono.Library.Rendering;
using SlaamMono.Library.Rendering.Text;
using SlaamMono.Resources;
using System.Collections.Generic;

namespace SlaamMono.Graphing
{
    public class Graph
    {
        private Rectangle GraphRectangle;
        private int Gap;
        public GraphItemCollection Items = new GraphItemCollection();
        private GraphDrawingBlockCollection Drawings = new GraphDrawingBlockCollection();
        private List<GraphWritingString> StringsToWrite = new List<GraphWritingString>();
        private Color ColorToDraw;

        public Graph(Rectangle graphrect, int gap, Color coltodraw)
        {
            GraphRectangle = graphrect;
            Gap = gap;
            ColorToDraw = coltodraw;
        }

        public void CalculateBlocks()
        {
            Drawings.Clear();
            StringsToWrite.Clear();
            int SizeOfColumns = GraphRectangle.Width / Items.Columns.Count;
            int HeightOfRows = 30;

            int ColumnWidth = SizeOfColumns - Gap;
            int RowHeight = HeightOfRows - Gap;

            int XOffset = ColumnWidth + Gap;
            int YOffset = RowHeight + Gap;

            for (int x = 0; x < Items.Columns.Count; x++)
            {
                Rectangle NewBlock = new Rectangle(GraphRectangle.X + XOffset * x, GraphRectangle.Y, ColumnWidth, RowHeight);

                if (Items.Columns[x].Trim() != "")
                {
                    Drawings.Add(new GraphDrawingBlock(NewBlock, ColorToDraw));

                    StringsToWrite.Add(new GraphWritingString(Items.Columns[x],
                        new Vector2(NewBlock.X + NewBlock.Width / 2, NewBlock.Y + NewBlock.Height / 2)));
                }
            }

            for (int x = 0; x < Items.Count; x++)
            {
                for (int y = 0; y < Items.Columns.Count; y++)
                {
                    if (Items[x].Details.Count >= Items.Columns.Count)
                    {
                        if (Items[x].Details[y].Trim() != "")
                        {
                            Rectangle NewBlock = new Rectangle(GraphRectangle.X + XOffset * y, GraphRectangle.Y + YOffset * (1 + x), ColumnWidth, RowHeight);
                            if (Items[x].Highlight)
                                Drawings.Add(new GraphDrawingBlock(NewBlock, new Color((byte)135, (byte)206, (byte)250, ColorToDraw.A)));
                            else
                                Drawings.Add(new GraphDrawingBlock(NewBlock, ColorToDraw));

                            StringsToWrite.Add(new GraphWritingString(Items[x].Details[y],
                                new Vector2(NewBlock.X + NewBlock.Width / 2, NewBlock.Y + NewBlock.Height / 2)));
                        }
                    }
                }
            }
        }

        public void SetHighlight(int index)
        {
            for (int x = 0; x < Items.Count; x++)
                Items[x].Highlight = false;
            Items[index].Highlight = true;
            CalculateBlocks();
        }

        public void Draw(SpriteBatch batch)
        {
            Drawings.Draw(batch);
            for (int x = 0; x < StringsToWrite.Count; x++)
            {
                RenderGraphManager.Instance.RenderText(StringsToWrite[x].Str, StringsToWrite[x].Pos, ResourceManager.Instance.GetFont("SegoeUIx14pt"), Color.White, TextAlignment.Centered, true);
            }
        }
    }
}
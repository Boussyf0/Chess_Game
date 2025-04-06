using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows; // This brings in the Application class and other WPF types.


namespace ChesseUI
{
    public static class CheesCursors
    {
        public static readonly Cursor WhiteCursor = LoadCursor("Assets/CursorW.cur");
        public static readonly Cursor BlackCursor = LoadCursor("Assets/CursorB.cur");
        private static Cursor LoadCursor (string filePath)
        {
            Stream stream = Application.GetResourceStream (new Uri (filePath, UriKind.Relative)).Stream;
            return new Cursor (stream, true);
        }
    }
}

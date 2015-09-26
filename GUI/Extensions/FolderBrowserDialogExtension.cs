namespace GUI.Extensions
{
    public static class FolderBrowserDialogExtension
    {
        public static System.Windows.Forms.IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            public OldWindow(System.IntPtr handle)
            {
                Handle = handle;
            }
            
            public System.IntPtr Handle { get; }
        }

    }
}

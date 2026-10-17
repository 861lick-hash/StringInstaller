namespace StarStringsInstaller;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (sender, e) =>
        {
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "StarStrings Installer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"An unhandled domain error occurred: {ex.Message}\n\n{ex.StackTrace}", "StarStrings Installer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
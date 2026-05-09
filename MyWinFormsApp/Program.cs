namespace MyWinFormsApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To apply ASP.NET Core configuration to Windows Forms,
            // use the Microsoft.Extensions.Hosting package.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}

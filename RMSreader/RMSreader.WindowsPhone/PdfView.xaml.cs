using pdftron;
using pdftron.PDF;
using pdftron.PDF.Tools;
using pdftron.SDF;
using RMSreader.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace RMSreader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PdfView
    {
        private NavigationHelper navigationHelper;
        private PDFViewCtrl pdfViewCtrl;
        private ToolManager MyToolManager;
        private PDFDoc pdfDoc;

        public PdfView()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            PDFNet.Initialize();
            pdfViewCtrl = new PDFViewCtrl();
            PDFViewBorder.Child = pdfViewCtrl;
            MyToolManager = new ToolManager(pdfViewCtrl);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            if (App.importedPdfFile != null)
            {
                try
                {
                    pdfDoc = new PDFDoc(App.importedPdfFile);
                    pdfViewCtrl.SetDoc(pdfDoc);
                }
                catch (Exception)
                {
                    App.ShowErrorDialog("Das PDF Dokument konnte nicht geöffnet werden.", "Fehler beim Verarbeiten");
                }
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            if (e.NavigationMode == NavigationMode.Back && pdfDoc != null)
            {
                MessageDialog dialog = new MessageDialog("Sollen mögliche Änderungen gespeichert werden?", "Warnung");
                dialog.Commands.Add(new UICommand("Ja", command =>
                {
                    OverwriteOldDocument();
                }));
                dialog.Commands.Add(new UICommand("Nain"));
                await dialog.ShowAsync();
            }

            PDFNet.Terminate();
        }

        private async void OverwriteOldDocument()
        {
            await pdfDoc.SaveAsync();
        }
    }
}

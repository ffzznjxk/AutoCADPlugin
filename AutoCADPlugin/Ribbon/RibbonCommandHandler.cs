using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using System;
using System.Windows.Input;

namespace AutoCADPlugin
{
    internal class RibbonCommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if (parameter is RibbonButton)
            {
                RibbonButton btn = parameter as RibbonButton;
                if (btn.CommandParameter != null)
                    doc.SendStringToExecute($"{btn.CommandParameter}\n", true, false, false);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace EasySaveDesktop.Converters
{
    class BackupConfigWizardStepToViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value switch
            {
                1 => new BackupConfigWizardStep1(),
                2 => new BackupConfigWizardStep2(),
                3 => new BackupConfigWizardStep3(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingNotification.ExtractError(value);
        }
    }
}

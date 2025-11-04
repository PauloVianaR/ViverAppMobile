using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Converters
{
    public class FormValidationValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string field || parameter is not string validation)
                return false;

            return validation switch
            {
                "Email" => !ValidationHelper.IsEmailValid(field),
                "Password" => !ValidationHelper.IsPasswordValid(field),
                "CPF" => !ValidationHelper.IsCPFValid(field),
                "CNPJ" => !ValidationHelper.IsCnpjValid(field),
                "CEP" => !ValidationHelper.IsValidCep(field),
                "Phone" => !ValidationHelper.IsPhoneValid(field),
                "CRM" => !ValidationHelper.IsCRMValid(field),
                "Number" => !ValidationHelper.IsNumber(field),
                "NotNumberOrEmpty" => !ValidationHelper.IsNotNumberOrEmpty(field),
                "BrState" => !ValidationHelper.IsBrStateValid(field),
                "NotEmpty" => string.IsNullOrWhiteSpace(field),
                _ => (object)false,
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }

    }
}

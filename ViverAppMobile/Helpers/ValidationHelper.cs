using System.Text.RegularExpressions;

namespace ViverAppMobile.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if(!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return false;

            return true;
        }

        public static bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        public static bool IsPhoneValid(string phone)
        {
            return Regex.IsMatch(phone ?? "", @"^\(\d{2}\) \d{4,5}-\d{4}$");
        }

        public static bool IsCRMValid(string crm)
        {
            return !string.IsNullOrWhiteSpace(crm) && crm.Length >= 4;
        }

        public static bool IsNumber(string? number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            for (int i = 0; i < number.Length; i++)
            {
                char c = number[i];
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsNotNumberOrEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsCPFValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var regex = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
            if (!regex.IsMatch(cpf))
                return false;

            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            if (new string(cpf[0], cpf.Length) == cpf)
                return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(cpf[i]))
                    return false;
                sum += (cpf[i] - '0') * (10 - i);
            }

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (cpf[9] - '0' != digit1)
                return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            if (cpf[10] - '0' != digit2)
                return false;

            return true;
        }

        public static bool IsCnpjValid(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            cnpj = new string([.. cnpj.Where(char.IsDigit)]);

            if (cnpj.Length != 14)
                return false;

            if (new string(cnpj[0], cnpj.Length) == cnpj)
                return false;

            int[] multiplier1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplier2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            string hasCnpj = cnpj[..12];
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(hasCnpj[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            if (remainder < 2) remainder = 0; else remainder = 11 - remainder;

            string digit = remainder.ToString();
            hasCnpj += digit;
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(hasCnpj[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            if (remainder < 2) remainder = 0; else remainder = 11 - remainder;

            digit += remainder.ToString();

            return cnpj.EndsWith(digit);
        }

        public static bool IsValidCep(string? cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return false;

            Regex _regexCep = new(@"^\d{5}-\d{3}$");
            return _regexCep.IsMatch(cep);
        }

        public static bool IsBrStateValid(string? state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return false;

            HashSet<string> ValidStates = new(StringComparer.OrdinalIgnoreCase)
            {
                "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES",
                "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR",
                "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC",
                "SP", "SE", "TO"
            };

            return ValidStates.Contains(state.Trim());
        }

        public static string CapitalizeName(string rawname)
        {
            if (string.IsNullOrWhiteSpace(rawname))
                return string.Empty;

            string[] prepositions =
            [
                "de", "da", "do", "das", "dos", "e", "em", "no", "na", "nos", "nas", "a", "o"
            ];

            var words = rawname.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];

                if (i == 0 || !prepositions.Contains(word))
                {
                    words[i] = char.ToUpper(word[0]) + word.Substring(1);
                }
            }

            return string.Join(' ', words).Trim();
        }
    }
}

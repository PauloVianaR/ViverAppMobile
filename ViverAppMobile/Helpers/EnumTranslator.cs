using ViverApp.Shared.Models;
using AppointmentType = ViverAppMobile.Models.AppointmentType;

namespace ViverAppMobile.Helpers
{
    public class EnumTranslator
    {
        public static string TranslatePaymentType(int method) => TranslatePaymentType((PayMethod)method);
        public static string TranslatePaymentType(PayMethod method)
        {
            return method switch
            {
                PayMethod.Card => "Cartão",
                PayMethod.CREDIT_CARD => "Cartão de Crédito",
                PayMethod.DEBIT_CARD => "Cartão de Débito",
                PayMethod.Cash => "Dinheiro",
                PayMethod.BankSlip => "Boleto",
                _ => method.ToString()
            };
        }

        public static string TranslateAppointmentType(int type) => TranslateAppointmentType((AppointmentType)type);
        public static string TranslateAppointmentType(AppointmentType type)
        {
            return type switch
            {
                AppointmentType.Consultation => "Consulta",
                AppointmentType.Examination => "Exame",
                AppointmentType.Surgery => "Cirurgia",
                _ => type.ToString()
            }; 
        }

        public static string TranslateNotificationType(int type) => TranslateNotificationType((NotificationType)type);
        public static string TranslateNotificationType(NotificationType type)
        {
            return type switch
            {
                NotificationType.UpdatedSystem => "Sistema atualizado",
                NotificationType.AwaitingApproval => "Novo usuário aguardando aprovação",
                NotificationType.PendingPayment => "Atendimento confirmado com pagamento pendente",
                NotificationType.RescheduledAppointment => "Atendimento reagendado",
                NotificationType.CanceledAppointment => "Atendimento cancelado",
                NotificationType.FinishedAppointment => "Atendimento finalizado com sucesso",
                NotificationType.PaymentSuccess => "Pagamento realizado com sucesso",
                _ => type.ToString()
            };
        }

        public static string TranslateUserType(int type) => TranslateUserType((UserType)type);
        public static string TranslateUserType(UserType type)
        {
            return type switch
            {
                UserType.None => "Indefinido",
                UserType.Admin => "Administrador",
                UserType.Patient => "Paciente",
                UserType.Manager => "Gerente",
                UserType.Doctor => "Médico",
                _ => type.ToString()
            };
        }

        public static string TranslateSeverity(int severity) => TranslateSeverity((Severity)severity);
        public static string TranslateSeverity(Severity severity)
        {
            return severity switch
            {
                Severity.None => "Sem Prioridade",
                Severity.Low => "Baixa",
                Severity.Medium => "Média",
                Severity.High => "Alta",
                _ => severity.ToString()
            };
        }

        public static string TranslateUserStatus(int status) => TranslateUserStatus((UserStatus)status);
        public static string TranslateUserStatus(UserStatus status)
        {
            return status switch
            {
                UserStatus.All => "Todos",
                UserStatus.Active => "Ativo",
                UserStatus.PendingApproval => "Pendente de Aprovação pelo Administrador",
                UserStatus.Rejected => "Cadastro Rejeitado",
                UserStatus.Blocked => "Cadastro Bloqueado",
                UserStatus.PendingEmail => "Email Pendente de Confirmação",
                _ => status.ToString()
            };
        }

        public static string TranslateScheduleStatus(int status, AppointmentType type)
        {
            if (type == AppointmentType.Examination)
                return ScheduleStatusMaleTranslateByInt(status);

            return ScheduleStatuesFemaleTranslateByInt(status);
        }

        private static string ScheduleStatusMaleTranslateByInt(int status)
        {
            return status switch
            {
                1 => "pendente",
                2 => "confirmado",
                3 => "realizado",
                5 => "cancelado",
                _ => "falha na tradução"
            };
        }

        private static string ScheduleStatuesFemaleTranslateByInt(int status)
        {
            return status switch
            {
                1 => "pendente",
                2 => "confirmada",
                3 => "realizada",
                5 => "cancelada",
                _ => "falha na tradução"
            };
        }
    }
}

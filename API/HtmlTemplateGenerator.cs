using Domain;

namespace API
{
    public class HtmlTemplateGenerator : IHtmlTemplateGenerator
    {
        public string GenerateBodyFor(EmailTemplateTypes templateType)
        {
            switch (templateType)
            {
                case EmailTemplateTypes.PasswordReset:
                    return PasswordResetEmailBody();
                case EmailTemplateTypes.VerifyEmail:
                    return VerifyEmailBody();
                case EmailTemplateTypes.ChangeEmail:
                    return ChangeEmailBody();
                default:
                    return string.Empty;
            }
        }

        private static string PasswordResetEmailBody()
        {
            return $@"
             <html lang='en'>
             <head>     
            <meta charset='utf-8'>
            <meta name='viewport' content='initial-scale=1, maximum-scale=1'>
            <meta name='viewport' content='width=device-width, initial-scale=1'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto:300,400,500' rel='stylesheet'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto&display=swap' rel='stylesheet'>
            </head>
            <body style='height: 100vh; font-family: 'Roboto', sans-serif; color:white; text-align: center'>
              <div>Please reset your password by clicking here: <a href={nameof(ApiConstants.Email.CallbackUrlToken)}>Reset Password</a></div>
            </body>
            </html>            
            ";
        }

        private static string VerifyEmailBody()
        {
            return $@"
             <html lang='en'>
             <head>     
            <meta charset='utf-8'>
            <meta name='viewport' content='initial-scale=1, maximum-scale=1'>
            <meta name='viewport' content='width=device-width, initial-scale=1'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto:300,400,500' rel='stylesheet'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto&display=swap' rel='stylesheet'>
            </head>
            <body style='height: 100vh; font-family: 'Roboto', sans-serif; color:white; text-align: center'>
              <div>Please verify your account by clicking here: <a href={nameof(ApiConstants.Email.CallbackUrlToken)}>Verify Email</a></div>
            </body>
            </html>            
            ";
        }

        private static string ChangeEmailBody()
        {
            return $@"
             <html lang='en'>
             <head>     
            <meta charset='utf-8'>
            <meta name='viewport' content='initial-scale=1, maximum-scale=1'>
            <meta name='viewport' content='width=device-width, initial-scale=1'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto:300,400,500' rel='stylesheet'>  
            <link href='https://fonts.googleapis.com/css?family=Roboto&display=swap' rel='stylesheet'>
            </head>
            <body style='height: 100vh; font-family: 'Roboto', sans-serif; color:white; text-align: center'>
              <div>Please confirm your e-mail change request by clicking here: <a href={nameof(ApiConstants.Email.CallbackUrlToken)}>Confirm e-mail change</a></div>
            </body>
            </html>            
            ";
        }
    }
}

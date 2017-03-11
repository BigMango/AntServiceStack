namespace AntServiceStack.FluentValidation.Validators
{
    using System.Linq;
    using Resources;


    /// <summary>
    /// Ensures that the property value is a valid credit card number.
    /// </summary>
    public class CreditCardValidator : PropertyValidator {
        // This logic was taken from the CreditCardAttribute in the ASP.NET MVC3 source.

        public CreditCardValidator() : base(() => Messages.CreditCardError, ValidationErrors.CreditCard) {
        }

        protected override bool IsValid(PropertyValidatorContext context) {
            var value = context.PropertyValue as string;

            if (value == null) {
                return true;
            }

            value = value.Replace("-", "");

            int checksum = 0;
            bool evenDigit = false;

            // http://www.beachnet.com/~hstiles/cardtype.html
            foreach (char digit in value.Reverse()) {
                if (!char.IsDigit(digit)) {
                    return false;
                }

                int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
                evenDigit = !evenDigit;

                while (digitValue > 0) {
                    checksum += digitValue % 10;
                    digitValue /= 10;
                }
            }

            return (checksum % 10) == 0;
        }
    }
}
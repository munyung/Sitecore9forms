using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.ExperienceForms.Mvc.Models.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Mvc;

namespace Sitecore.Scientist.Forms.Validator
{
    public class CaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorMessage { get; set; }
    }

    public class CaptchaValidator : ValidationElement<string>
    {

        public override IEnumerable<ModelClientValidationRule> ClientValidationRules
        {
            get
            {
                if (string.IsNullOrEmpty(this.PrivateKey))
                {
                    yield break;
                }
            }
        }

        protected virtual string PrivateKey
        {
            get;
            set;
        }

        protected virtual string Title
        {
            get;
            set;
        }
        protected virtual string FieldName
        {
            get;
            set;
        }
        public CaptchaValidator(ValidationDataModel validationItem) : base(validationItem)
        {

        }

        public override void Initialize(object validationModel)
        {
            base.Initialize(validationModel);
            StringInputViewModel stringInputViewModel = validationModel as StringInputViewModel;
            if (stringInputViewModel != null)
            {
                var fieldItem = Context.Database.GetItem(ID.Parse(stringInputViewModel.ItemId));
               if(fieldItem!=null)
                {
                    this.PrivateKey = fieldItem["Private Key"];
                }
                this.Title = stringInputViewModel.Title;
                this.FieldName = stringInputViewModel.Name;
            }
        }

        public override ValidationResult Validate(object value)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var isCaptchaValid = ValidateCaptcha((string)value, this.PrivateKey);
            if (!isCaptchaValid)
            {
                return new ValidationResult(this.FormatMessage(new object[] { this.Title }));
            }
            return ValidationResult.Success;
        }
        public static bool ValidateCaptcha(string response, string secret)
        {
            var client = new WebClient();
            var reply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));

            var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(reply);

            return System.Convert.ToBoolean(captchaResponse.Success);
        }
    }
}

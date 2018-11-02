using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.ExperienceForms.Data;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.ExperienceForms.Mvc.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Sitecore.Scientist.Forms.Validator
{
    public class EmailExistValidator : ValidationElement<string>
    {
        public readonly string FormTemplateId = "{6ABEE1F2-4AB4-47F0-AD8B-BDB36F37F64C}";

        private IFormDataProvider _dataProvider;


        protected virtual IFormDataProvider FormDataProvider
        {
            get
            {
                IFormDataProvider formDataProvider = this._dataProvider;
                if (formDataProvider == null)
                {
                    IFormDataProvider service = ServiceLocator.ServiceProvider.GetService<IFormDataProvider>();
                    IFormDataProvider formDataProvider1 = service;
                    this._dataProvider = service;
                    formDataProvider = formDataProvider1;
                }
                return formDataProvider;
            }
        }

        public override IEnumerable<ModelClientValidationRule> ClientValidationRules
        {
            get
            {
                if (string.IsNullOrEmpty(this.FormId))
                {
                    yield break;
                }
            }
        }

        protected virtual string FormId
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
        public EmailExistValidator(ValidationDataModel validationItem) : base(validationItem)
        {

        }

        public override void Initialize(object validationModel)
        {
            base.Initialize(validationModel);
            StringInputViewModel stringInputViewModel = validationModel as StringInputViewModel;
            if (stringInputViewModel != null)
            {
                var fieldItem = Context.Database.GetItem(ID.Parse(stringInputViewModel.ItemId));
                var formItem = GetFormItem(fieldItem);
                if (formItem != null)
                {
                    this.FormId = formItem.ID.ToString();
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
            if (!string.IsNullOrEmpty(this.FormId))
            {
                var formId = Guid.Parse(this.FormId);
                var data = this.FormDataProvider.GetEntries(formId, null, null);
                foreach (var item in data)
                {
                    var emailValue = item.Fields.Where(x => x.FieldName == this.FieldName).FirstOrDefault();
                    if (emailValue != null && emailValue.Value.ToLower() == value.ToString().ToLower())
                    {
                        return new ValidationResult(this.FormatMessage(new object[] { this.Title }));
                    }

                }
                return ValidationResult.Success;
            }
            return new ValidationResult(this.FormatMessage(new object[] { this.Title }));
        }
        public Item GetFormItem(Item sitecoreItem)
        {
            if (sitecoreItem == null)
                return null;
            Item parent = sitecoreItem.Parent;
            if (parent.TemplateID == ID.Parse(this.FormTemplateId))
            {
                return parent;
            }
            else if (ItemIDs.RootID == parent.ID)
            {
                return null;
            }
            else
            {
                return GetFormItem(parent);
            }
        }
    }
}

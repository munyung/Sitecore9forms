using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using System.Globalization;

namespace Sitecore.Scientist.Forms.ViewModel
{
    public class EmailInputViewModel : StringInputViewModel
    {
        public string FormId
        {
            get;
            set;
        }
        public EmailInputViewModel()
        {

        }
        protected override void InitItemProperties(Item item)
        {
            var formItem = GetFormItem(item);
            if (formItem != null)
            {
                this.FormId = formItem.ID.ToString();
            }
        }

        protected override void UpdateItemFields(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            base.UpdateItemFields(item);
        }

        public Item GetFormItem(Item sitecoreItem)
        {
            Item parent = sitecoreItem.Parent;
            if (parent.TemplateID == ID.Parse("{6ABEE1F2-4AB4-47F0-AD8B-BDB36F37F64C}"))
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
